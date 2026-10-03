using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartDocs.Application.Interfaces;
using SmartDocs.Domain.Enums;
using SmartDocs.Infrastructure.Persistence;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using SmartDocs.Domain.Entities;

namespace SmartDocs.Api.Controllers
{
    [ApiController]
    [Route("api/documents")]
    [Authorize]
    public class DocumentsController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IFileStorage _storage;

        public DocumentsController(AppDbContext db, IFileStorage storage)
        {
            _db = db;
            _storage = storage;
        }

        // Current user ka ID nikaalo JWT token se
        private Guid UserId => Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost]
        [RequestSizeLimit(20 * 1024 * 1024)]  // 20 MB max
        public async Task<IActionResult> Upload(IFormFile file, CancellationToken ct)
        {
            // 1. Validation
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "File required" });

            if (file.Length > 20 * 1024 * 1024)
                return BadRequest(new { message = "Max 20 MB allowed" });

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext != ".pdf" && ext != ".docx" && ext != ".txt")
                return BadRequest(new { message = "PDF/DOCX/TXT only" });

            // 2. Save to storage
            using var stream = file.OpenReadStream();
            var url = await _storage.SaveAsync(stream, file.FileName, ct);

            // 3. Save record in database
            var doc = new Document
            {
                Id = Guid.NewGuid(),
                UserId = UserId,
                FileName = file.FileName,
                ContentType = file.ContentType,
                SizeBytes = file.Length,
                BlobUrl = url,
                Status = DocumentStatus.Uploaded,
                CreatedAt = DateTime.UtcNow
            };

            _db.Documents.Add(doc);
            await _db.SaveChangesAsync(ct);

            return Ok(new
            {
                doc.Id,
                doc.FileName,
                doc.SizeBytes,
                Status = doc.Status.ToString(),
                doc.CreatedAt
            });
        }

        [HttpGet]
        public async Task<IActionResult> List(CancellationToken ct)
        {
            var docs = await _db.Documents
                .Where(d => d.UserId == UserId)
                .OrderByDescending(d => d.CreatedAt)
                .Select(d => new
                {
                    d.Id,
                    d.FileName,
                    d.ContentType,
                    d.SizeBytes,
                    Status = d.Status.ToString(),
                    d.CreatedAt
                })
                .ToListAsync(ct);

            return Ok(docs);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            // Ownership check: sirf apne documents
            var doc = await _db.Documents
                .FirstOrDefaultAsync(d => d.Id == id && d.UserId == UserId, ct);

            if (doc == null)
                return NotFound();

            // 1. Delete from storage
            await _storage.DeleteAsync(doc.BlobUrl, ct);

            // 2. Delete from database
            _db.Documents.Remove(doc);
            await _db.SaveChangesAsync(ct);

            return NoContent();
        }
    }
}
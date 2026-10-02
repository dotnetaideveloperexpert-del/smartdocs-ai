using SmartDocs.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartDocs.Domain.Entities
{
    public class Document
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long SizeBytes { get; set; }
        public string BlobUrl { get; set; } = string.Empty;
        public DocumentStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public User User { get; set; } = null!;
        public ICollection<DocumentChunk> Chunks { get; set; } = new List<DocumentChunk>();
    }
}

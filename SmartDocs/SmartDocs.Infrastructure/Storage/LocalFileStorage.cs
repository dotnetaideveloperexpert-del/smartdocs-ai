using SmartDocs.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartDocs.Infrastructure.Storage
{
    public class LocalFileStorage : IFileStorage
    {
        private const string Root = "wwwroot/uploads";

        public async Task<string> SaveAsync(Stream stream, string fileName, CancellationToken ct = default)
        {
            Directory.CreateDirectory(Root);

            // Unique name (GUID + original extension)
            var stored = $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";
            var fullPath = Path.Combine(Root, stored);

            using var fs = File.Create(fullPath);
            await stream.CopyToAsync(fs, ct);

            // URL path return karo (storage-agnostic)
            return $"/uploads/{stored}";
        }

        public Task<Stream> GetAsync(string path, CancellationToken ct = default)
        {
            var fullPath = Path.Combine(Root, Path.GetFileName(path));
            Stream stream = File.OpenRead(fullPath);
            return Task.FromResult(stream);
        }

        public Task DeleteAsync(string path, CancellationToken ct = default)
        {
            var fullPath = Path.Combine(Root, Path.GetFileName(path));
            if (File.Exists(fullPath))
                File.Delete(fullPath);
            return Task.CompletedTask;
        }
    }
}

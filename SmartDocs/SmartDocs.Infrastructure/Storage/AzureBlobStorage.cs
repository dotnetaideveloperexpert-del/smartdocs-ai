using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using SmartDocs.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartDocs.Infrastructure.Storage
{
    public class AzureBlobStorage : IFileStorage
    {
        private readonly BlobContainerClient _container;
        public AzureBlobStorage(IConfiguration configuration)
        {
            var connectionString = configuration["AzureBlob:ConnectionString"]!;
            var containerName = configuration["AzureBlob:Container"] ?? "uploads";

            var blobServiceClient = new BlobServiceClient(connectionString);
            _container = blobServiceClient.GetBlobContainerClient(containerName);
            _container.CreateIfNotExists();
        }
        public async Task DeleteAsync(string path, CancellationToken ct = default)
        {
            try
            {
                var fileName = Path.GetFileName(new Uri(path).LocalPath);
                var blobClient = _container.GetBlobClient(fileName);
                await blobClient.DeleteIfExistsAsync(cancellationToken: ct);
            }
            catch (Exception ex)
            {
                // Log warning but don't fail the entire operation
                Console.WriteLine($"Blob delete warning: {ex.Message}");
            }
        }

        public async Task<Stream> GetAsync(string path, CancellationToken ct = default)
        {
            // Extract just the file name from URL
            var fileName = Path.GetFileName(new Uri(path).LocalPath);
            var blobClient = _container.GetBlobClient(fileName);

            var memoryStream = new MemoryStream();
            await blobClient.DownloadToAsync(memoryStream, ct);
            memoryStream.Position = 0;
            return memoryStream;
        }

        public async Task<string> SaveAsync(Stream stream, string fileName, CancellationToken ct = default)
        {
            var stored = $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";
            var blobClient = _container.GetBlobClient(stored);
            await blobClient.UploadAsync(stream, overwrite: true, cancellationToken: ct);

            // Return the blob URI (public path)
            return blobClient.Uri.ToString();
        }
    }
}

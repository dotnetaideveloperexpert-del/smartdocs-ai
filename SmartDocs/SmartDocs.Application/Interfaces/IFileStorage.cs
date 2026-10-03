using System;
using System.Collections.Generic;
using System.Text;

namespace SmartDocs.Application.Interfaces
{
    public interface IFileStorage
    {
        Task<string> SaveAsync(Stream stream, string fileName, CancellationToken ct = default);
        Task<Stream> GetAsync(string path, CancellationToken ct = default);
        Task DeleteAsync(string path, CancellationToken ct = default);
    }
}

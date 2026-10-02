using System;
using System.Collections.Generic;
using System.Text;

namespace SmartDocs.Domain.Entities
{
    public class DocumentChunk
    {
        public Guid Id { get; set; }
        public Guid DocumentId { get; set; }
        public int ChunkIndex { get; set; }
        public string Content { get; set; } = string.Empty;
        public int TokenCount { get; set; }

        // Navigation property
        public Document Document { get; set; } = null!;
    }
}

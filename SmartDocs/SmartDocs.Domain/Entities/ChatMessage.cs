using System;
using System.Collections.Generic;
using System.Text;

namespace SmartDocs.Domain.Entities
{
    public class ChatMessage
    {
        public Guid Id { get; set; }
        public Guid SessionId { get; set; }
        public string Role { get; set; } = string.Empty; // "user" or "assistant"
        public string Content { get; set; } = string.Empty;
        public string? SourcesJson { get; set; } // Citations JSON
        public DateTime CreatedAt { get; set; }

        // Navigation property
        public ChatSession Session { get; set; } = null!;
    }
}

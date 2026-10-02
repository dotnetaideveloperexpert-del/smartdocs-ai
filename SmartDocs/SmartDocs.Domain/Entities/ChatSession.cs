using System;
using System.Collections.Generic;
using System.Text;

namespace SmartDocs.Domain.Entities
{
    public class ChatSession
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public User User { get; set; } = null!;
        public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}

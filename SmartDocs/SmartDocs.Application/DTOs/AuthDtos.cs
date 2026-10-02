using System;
using System.Collections.Generic;
using System.Text;

namespace SmartDocs.Application.DTOs
{
    public class AuthDtos
    {
        // Register request
        public record RegisterRequest(string Name, string Email, string Password);

        // Login request
        public record LoginRequest(string Email, string Password);

        // Auth response (register/login ke baad)
        public record AuthResponse(Guid UserId, string Name, string Email, string Role, string Token);
    }
}

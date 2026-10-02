using SmartDocs.Application.Interfaces;
using SmartDocs.Domain.Entities;
using SmartDocs.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using static SmartDocs.Application.DTOs.AuthDtos;

namespace SmartDocs.Application.Services
{
    public class AuthService
    {
        private readonly IUserRepository _users;
        private readonly IJwtService _jwt;

        public AuthService(IUserRepository users, IJwtService jwt)
        {
            _users = users;
            _jwt = jwt;
        }

        public async Task<AuthResponse?> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
        {
            // 1. Email already exists?
            if (await _users.EmailExistsAsync(request.Email, ct))
                return null;

            // 2. User banao with hashed password
            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Email = request.Email.ToLower().Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = UserRole.User,
                CreatedAt = DateTime.UtcNow
            };

            // 3. Database mein save
            await _users.AddAsync(user, ct);

            // 4. Token generate karo
            var token = _jwt.GenerateToken(user);

            return new AuthResponse(user.Id, user.Name, user.Email, user.Role.ToString(), token);
        }

        public async Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default)
        {
            // 1. User dhundo
            var user = await _users.GetByEmailAsync(request.Email.ToLower().Trim(), ct);
            if (user == null) return null;

            // 2. Password verify karo
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return null;

            // 3. Token generate karo
            var token = _jwt.GenerateToken(user);

            return new AuthResponse(user.Id, user.Name, user.Email, user.Role.ToString(), token);
        }
    }
}

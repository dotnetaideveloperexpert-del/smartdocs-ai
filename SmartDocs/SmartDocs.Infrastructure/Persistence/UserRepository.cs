using Microsoft.EntityFrameworkCore;
using SmartDocs.Application.Interfaces;
using SmartDocs.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartDocs.Infrastructure.Persistence
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _db;

        public UserRepository(AppDbContext db)
        {
            _db = db;
        }
        public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => _db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

        public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

        public async Task AddAsync(User user, CancellationToken ct = default)
        {
            _db.Users.Add(user);
            await _db.SaveChangesAsync(ct);
        }

        public Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
            => _db.Users.AnyAsync(u => u.Email == email, ct);
    }
}

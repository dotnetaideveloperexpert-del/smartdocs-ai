using Microsoft.EntityFrameworkCore;
using SmartDocs.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartDocs.Infrastructure.Persistence
{
    public class AppDbContext:DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Tables
        public DbSet<User> Users => Set<User>();
        public DbSet<Document> Documents => Set<Document>();
        public DbSet<DocumentChunk> DocumentChunks => Set<DocumentChunk>();
        public DbSet<ChatSession> ChatSessions => Set<ChatSession>();
        public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); 

            // User: Email unique hona chahiye
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Document → User (one user has many documents)
            modelBuilder.Entity<Document>()
                .HasOne(d => d.User)
                .WithMany(u => u.Documents)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // DocumentChunk → Document
            modelBuilder.Entity<DocumentChunk>()
                .HasOne(c => c.Document)
                .WithMany(d => d.Chunks)
                .HasForeignKey(c => c.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            // ChatSession → User
            modelBuilder.Entity<ChatSession>()
                .HasOne(s => s.User)
                .WithMany(u => u.ChatSessions)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ChatMessage → ChatSession
            modelBuilder.Entity<ChatMessage>()
                .HasOne(m => m.Session)
                .WithMany(s => s.Messages)
                .HasForeignKey(m => m.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes for performance
            modelBuilder.Entity<Document>()
                .HasIndex(d => d.UserId);

            modelBuilder.Entity<DocumentChunk>()
                .HasIndex(c => c.DocumentId);

            modelBuilder.Entity<ChatMessage>()
                .HasIndex(m => m.SessionId);
        }
    }
}

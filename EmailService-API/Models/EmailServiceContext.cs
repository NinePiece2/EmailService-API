using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using Npgsql;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using System.IO;

namespace EmailService_API.Models
{
    public partial class EmailServiceContext : DbContext, IDataProtectionKeyContext
    {
        public EmailServiceContext()
        {
        }

        public EmailServiceContext(DbContextOptions<EmailServiceContext> options)
            : base(options)
        {
        }

        public virtual DbSet<EnqueueIncomingMessage> EnqueueIncomingMessages { get; set; }
        public virtual DbSet<ApiKey> ApiKeys { get; set; }
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("dbo");

            modelBuilder.Entity<EnqueueIncomingMessage>().HasNoKey();
            
            // Explicitly map ApiKey columns to match PostgreSQL naming
            modelBuilder.Entity<ApiKey>(entity =>
            {
                entity.ToTable("apikeys");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Name).HasColumnName("name");
                entity.Property(e => e.KeyHash).HasColumnName("keyhash");
                entity.Property(e => e.KeyPrefix).HasColumnName("keyprefix");
                entity.Property(e => e.IsActive).HasColumnName("isactive");
                entity.Property(e => e.CreatedDate).HasColumnName("createddate");
                entity.Property(e => e.LastUsedDate).HasColumnName("lastuseddate");
                entity.Property(e => e.Description).HasColumnName("description");
            });
            
            // Explicitly map DataProtectionKey columns to match PostgreSQL naming
            modelBuilder.Entity<DataProtectionKey>(entity =>
            {
                entity.ToTable("dataprotectionkeys");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.FriendlyName).HasColumnName("friendlyname");
                entity.Property(e => e.Xml).HasColumnName("xml");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

        public int EnqueueIncomingMessagesRun(string userName, string title, string? CreatedEmail, string? CreatedName, bool? isSecure, string bodyHtml, string? messageType, bool? isImportantTag, string? ccEmail, string? bccEmail)
        {
            try
            {
                // Use SELECT to execute the PostgreSQL function with proper null handling
                var result = this.Database.ExecuteSqlRaw(
                    "SELECT dbo.enqueueincomingmessages({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9})",
                    userName ?? (object)"",
                    title ?? (object)"",
                    CreatedEmail ?? (object)"",
                    CreatedName ?? (object)"",
                    isSecure.HasValue ? (object)isSecure.Value : (object)false,
                    bodyHtml ?? (object)"",
                    messageType ?? (object)"",
                    isImportantTag.HasValue ? (object)isImportantTag.Value : (object)false,
                    ccEmail ?? (object)"",
                    bccEmail ?? (object)""
                );
                return 1;
            }
            catch (Exception ex)
            {
                // Log exception as needed
                Console.WriteLine($"Error executing enqueueincomingmessages: {ex.Message}");
                return -1;
            }
        }

    }
}

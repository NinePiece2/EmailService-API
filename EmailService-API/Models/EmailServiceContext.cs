using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace EmailService_API.Models
{
    public partial class EmailServiceContext : DbContext
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EnqueueIncomingMessage>().HasNoKey();
            modelBuilder.Entity<ApiKey>().HasKey(e => e.Id);

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

        public int EnqueueIncomingMessagesRun(string userName, string title, string? CreatedEmail, string? CreatedName, bool? isSecure, string bodyHtml, string? messageType, bool? isImportantTag, string? ccEmail, string? bccEmail)
        {
            var parameters = new List<Microsoft.Data.SqlClient.SqlParameter>
            {
                new Microsoft.Data.SqlClient.SqlParameter("@UserName", userName),
                new Microsoft.Data.SqlClient.SqlParameter("@Title", title),
                new Microsoft.Data.SqlClient.SqlParameter("@BodyHtml", bodyHtml),
            };

            AddNullableParameter(parameters, "@CreatedEmail", CreatedEmail);
            AddNullableParameter(parameters, "@CreatedName", CreatedName);
            AddNullableParameter(parameters, "@MessageType", messageType);
            AddNullableParameter(parameters, "@CCEmail", ccEmail);
            AddNullableParameter(parameters, "@BCCEmail", bccEmail);

            parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@IsSecure", (object)isSecure ?? DBNull.Value));
            parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@IsImportantTag", (object)isImportantTag ?? DBNull.Value));

            var sqlParameters = parameters.ToArray();

            try
            {
                // Use ExecuteSqlRaw to execute the command without expecting a result set.
                this.Database.ExecuteSqlRaw("EXEC EnqueueIncomingMessages @UserName, @Title, @CreatedEmail, @CreatedName, @IsSecure, @BodyHtml, @MessageType, @IsImportantTag, @CCEmail, @BCCEmail", sqlParameters);
                return 1;
            }
            catch (Exception e)
            {
                // Log exception as needed
                return -1;
            }
        }

        private void AddNullableParameter(List<Microsoft.Data.SqlClient.SqlParameter> parameters, string parameterName, string? parameterValue)
        {
            if (parameterValue != null)
            {
                parameters.Add(new Microsoft.Data.SqlClient.SqlParameter(parameterName, parameterValue));
            }
            else
            {
                parameters.Add(new Microsoft.Data.SqlClient.SqlParameter(parameterName, DBNull.Value));
            }
        }

    }
}

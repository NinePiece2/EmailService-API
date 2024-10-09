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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EnqueueIncomingMessage>().HasNoKey();


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

            // Handle nullable parameters
            AddNullableParameter(parameters, "@CreatedEmail", CreatedEmail);
            AddNullableParameter(parameters, "@CreatedName", CreatedName);
            AddNullableParameter(parameters, "@MessageType", messageType);
            AddNullableParameter(parameters, "@CCEmail", ccEmail);
            AddNullableParameter(parameters, "@BCCEmail", bccEmail);

            // Add isSecure parameter
            parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@IsSecure", (object)isSecure ?? DBNull.Value));
            parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@IsImportantTag", (object)isImportantTag ?? DBNull.Value));

            // Execute the stored procedure
            var sqlParameters = parameters.ToArray();
            try
            {
                EnqueueIncomingMessages.FromSqlRaw("EXEC EnqueueIncomingMessages @UserName, @Title, @CreatedEmail, @CreatedName, @IsSecure, @BodyHtml, @MessageType, @IsImportantTag, @CCEmail, @BCCEmail", sqlParameters).ToList();
                return 1;
            }
            catch (InvalidOperationException ex)
            {
                return 1;
            }
            catch (Exception e)
            {
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
                // Add a parameter with DBNull.Value if parameterValue is null
                parameters.Add(new Microsoft.Data.SqlClient.SqlParameter(parameterName, DBNull.Value));
            }
        }
    }
}

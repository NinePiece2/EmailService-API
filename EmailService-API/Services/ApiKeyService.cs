using EmailService_API.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace EmailService_API.Services
{
    public interface IApiKeyService
    {
        Task<(string apiKey, ApiKey keyRecord)> CreateApiKeyAsync(string name, string? description = null);
        Task<bool> ValidateApiKeyAsync(string apiKey);
        Task<ApiKey?> GetApiKeyByPrefixAsync(string prefix);
        Task UpdateLastUsedDateAsync(int keyId);
        Task<bool> RevokeApiKeyAsync(string keyPrefix);
        Task<List<ApiKey>> GetAllApiKeysAsync();
    }

    public class ApiKeyService : IApiKeyService
    {
        private readonly EmailServiceContext _context;
        private const string ApiKeyPrefix = "esapi_"; // EmailService API prefix

        public ApiKeyService(EmailServiceContext context)
        {
            _context = context;
        }

        public async Task<(string apiKey, ApiKey keyRecord)> CreateApiKeyAsync(string name, string? description = null)
        {
            // Generate a random 32-byte key
            var keyBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(keyBytes);
            }
            var keyString = Convert.ToBase64String(keyBytes).Replace("+", "").Replace("/", "").Replace("=", "");
            
            // Create the full API key with prefix
            var fullApiKey = $"{ApiKeyPrefix}{keyString}";
            
            // Get the prefix for storage (first 10 characters)
            var prefix = fullApiKey.Substring(0, 10);
            
            // Hash the full key for secure storage
            var keyHash = HashApiKey(fullApiKey);

            var apiKey = new ApiKey
            {
                Name = name,
                KeyHash = keyHash,
                KeyPrefix = prefix,
                Description = description,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            _context.ApiKeys.Add(apiKey);
            await _context.SaveChangesAsync();

            return (fullApiKey, apiKey);
        }

        public async Task<bool> ValidateApiKeyAsync(string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey) || !apiKey.StartsWith(ApiKeyPrefix))
            {
                return false;
            }

            var prefix = apiKey.Substring(0, Math.Min(10, apiKey.Length));
            var keyHash = HashApiKey(apiKey);

            var storedKey = await _context.ApiKeys
                .FirstOrDefaultAsync(k => k.KeyPrefix == prefix && k.IsActive);

            if (storedKey == null)
            {
                return false;
            }

            // Constant-time comparison to prevent timing attacks
            var isValid = CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(storedKey.KeyHash),
                Encoding.UTF8.GetBytes(keyHash)
            );

            if (isValid)
            {
                // Update last used date asynchronously without awaiting
                _ = UpdateLastUsedDateAsync(storedKey.Id);
            }

            return isValid;
        }

        public async Task<ApiKey?> GetApiKeyByPrefixAsync(string prefix)
        {
            return await _context.ApiKeys
                .FirstOrDefaultAsync(k => k.KeyPrefix == prefix);
        }

        public async Task UpdateLastUsedDateAsync(int keyId)
        {
            try
            {
                var apiKey = await _context.ApiKeys.FindAsync(keyId);
                if (apiKey != null)
                {
                    apiKey.LastUsedDate = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
            }
            catch
            {
                // Silently fail - LastUsedDate update is not critical
            }
        }

        public async Task<bool> RevokeApiKeyAsync(string keyPrefix)
        {
            var apiKey = await _context.ApiKeys
                .FirstOrDefaultAsync(k => k.KeyPrefix == keyPrefix);

            if (apiKey == null)
            {
                return false;
            }

            apiKey.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<ApiKey>> GetAllApiKeysAsync()
        {
            return await _context.ApiKeys
                .OrderByDescending(k => k.CreatedDate)
                .ToListAsync();
        }

        private string HashApiKey(string apiKey)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(apiKey));
                return Convert.ToBase64String(hashBytes);
            }
        }
    }
}

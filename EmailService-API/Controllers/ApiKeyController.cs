using EmailService_API.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmailService_API.Controllers
{
#if DEBUG
    [Route("[controller]")]
    [ApiController]
    public class ApiKeyController : ControllerBase
    {
        private readonly IApiKeyService _apiKeyService;
        private readonly IConfiguration _configuration;

        public ApiKeyController(IApiKeyService apiKeyService, IConfiguration configuration)
        {
            _apiKeyService = apiKeyService;
            _configuration = configuration;
        }

        /// <summary>
        /// Creates a new API key. Requires master key authentication.
        /// </summary>
        [HttpPost]
        [Route("Create")]
        public async Task<IActionResult> CreateApiKey([FromBody] CreateApiKeyRequest request, [FromHeader(Name = "X-Master-Key")] string? masterKey)
        {
            // Validate master key for creating new API keys
            var configuredMasterKey = _configuration["MasterKey"];
            if (string.IsNullOrEmpty(masterKey) || masterKey != configuredMasterKey)
            {
                return Unauthorized("Invalid master key");
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest("API key name is required");
            }

            try
            {
                var (apiKey, keyRecord) = await _apiKeyService.CreateApiKeyAsync(request.Name, request.Description);
                
                return Ok(new
                {
                    ApiKey = apiKey,
                    Name = keyRecord.Name,
                    Prefix = keyRecord.KeyPrefix,
                    CreatedDate = keyRecord.CreatedDate,
                    Message = "IMPORTANT: Save this API key securely. You won't be able to see it again!"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error creating API key: {ex.Message}");
            }
        }

        /// <summary>
        /// Lists all API keys (without revealing the actual keys). Requires master key authentication.
        /// </summary>
        [HttpGet]
        [Route("List")]
        public async Task<IActionResult> ListApiKeys([FromHeader(Name = "X-Master-Key")] string? masterKey)
        {
            var configuredMasterKey = _configuration["MasterKey"];
            if (string.IsNullOrEmpty(masterKey) || masterKey != configuredMasterKey)
            {
                return Unauthorized("Invalid master key");
            }

            try
            {
                var apiKeys = await _apiKeyService.GetAllApiKeysAsync();
                var result = apiKeys.Select(k => new
                {
                    k.Id,
                    k.Name,
                    k.KeyPrefix,
                    k.IsActive,
                    k.CreatedDate,
                    k.LastUsedDate,
                    k.Description
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error listing API keys: {ex.Message}");
            }
        }

        /// <summary>
        /// Revokes an API key by its prefix. Requires master key authentication.
        /// </summary>
        [HttpPost]
        [Route("Revoke")]
        public async Task<IActionResult> RevokeApiKey([FromBody] RevokeApiKeyRequest request, [FromHeader(Name = "X-Master-Key")] string? masterKey)
        {
            var configuredMasterKey = _configuration["MasterKey"];
            if (string.IsNullOrEmpty(masterKey) || masterKey != configuredMasterKey)
            {
                return Unauthorized("Invalid master key");
            }

            if (string.IsNullOrWhiteSpace(request.KeyPrefix))
            {
                return BadRequest("Key prefix is required");
            }

            try
            {
                var success = await _apiKeyService.RevokeApiKeyAsync(request.KeyPrefix);
                if (success)
                {
                    return Ok(new { Message = "API key revoked successfully" });
                }
                return NotFound("API key not found");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error revoking API key: {ex.Message}");
            }
        }
    }

    public class CreateApiKeyRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class RevokeApiKeyRequest
    {
        public string KeyPrefix { get; set; } = string.Empty;
    }
#endif
}

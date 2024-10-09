using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EmailService_API.Models;
using Microsoft.AspNetCore.Authorization;
namespace EmailService_API.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly EmailServiceContext context;
        public EmailController(EmailServiceContext _context)
        {
            context = _context;
        }

        [HttpPost]
        [Route("SendEmail")]
        public IActionResult SendEmail([FromBody] EnqueueIncomingMessage message)
        {
            try
            {
                var result = context.EnqueueIncomingMessagesRun(message.UserName, message.Title, message.CreatedEmail, message.CreatedName, message.IsSecure, message.BodyHtml, message.MessageType, message.IsImportantTag, message.CcEmail, message.BccEmail);

                if (result == 1)
                {
                    return Ok("Email Sent.");
                }
                return BadRequest("Failed to send email.");

            } catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}

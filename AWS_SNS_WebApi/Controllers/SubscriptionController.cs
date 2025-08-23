using AWS_ClassLibrary.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace AWS_SNS_WebApi.Controllers
{
    [Route("api/subscription")]
    [ApiController]
    public class SubscriptionController : ControllerBase
    {
        private readonly ILogger<SubscriptionController> _logger;
        private readonly SnsService _snsService;

        private readonly string _ARN;
        public SubscriptionController(ILogger<SubscriptionController> logger, SnsService snsService)
        {
            _logger = logger;
            _snsService = snsService;
        }

        [HttpPost("subscribe")]
        public async Task<IActionResult> Subscribe([EmailAddress] string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("Email is required.");
            }
            try
            {
                var message = await _snsService.Subscribe(email);
                return Ok(new { message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error subscribing email: {Email}", email);
                return StatusCode(500, "Internal server error while subscribing.");
            }
        }
    }
}

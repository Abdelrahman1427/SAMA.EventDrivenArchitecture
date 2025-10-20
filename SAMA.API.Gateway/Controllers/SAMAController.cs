using Microsoft.AspNetCore.Mvc;
using SAMA.EventBus;
using SAMA.AccountService.Commands;

namespace SAMA.API.Gateway.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SamaController : ControllerBase
    {
        private readonly IEventBus _eventBus;
        private readonly ILogger<SamaController> _logger;

        public SamaController(IEventBus eventBus, ILogger<SamaController> logger)
        {
            _eventBus = eventBus;
            _logger = logger;
        }

        [HttpPost("accounts")]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountRequest request)
        {
            try
            {
                var command = new CreateAccountCommand
                {
                    AccountNumber = request.AccountNumber,
                    CustomerId = request.CustomerId,
                    InitialBalance = request.InitialBalance,
                    Currency = request.Currency
                };

                await _eventBus.PublishAsync(command);

                return Accepted(new
                {
                    CommandId = command.EventId,
                    Status = "Processing",
                    Message = "Account creation request accepted"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating account through API Gateway");
                return StatusCode(500, new { Error = "Internal server error" });
            }
        }
    }

    public class CreateAccountRequest
    {
        public string AccountNumber { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public decimal InitialBalance { get; set; }
        public string Currency { get; set; } = "SAR";
    }
}
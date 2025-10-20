using Microsoft.AspNetCore.Mvc;
using SAMA.AccountService.Commands;
using SAMA.EventBus;

namespace SAMA.AccountService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController : ControllerBase
    {
        private readonly IEventBus _eventBus;
        private readonly ILogger<AccountsController> _logger;

        public AccountsController(IEventBus eventBus, ILogger<AccountsController> logger)
        {
            _eventBus = eventBus;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountCommand command)
        {
            try
            {
                await _eventBus.PublishAsync(command);
                return Accepted(new { CommandId = command.EventId, Status = "Processing" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating account");
                return StatusCode(500, new { Error = "Internal server error" });
            }
        }
    }
}
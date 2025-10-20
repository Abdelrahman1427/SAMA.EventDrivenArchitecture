using SAMA.AccountService.Commands;
using SAMA.EventBus;
using SAMA.SharedKernel.DomainEvents;

namespace SAMA.NotificationService.Handlers
{
    public class AccountCreatedEventHandler : IEventHandler<AccountCreatedEvent>
    {
        private readonly ILogger<AccountCreatedEventHandler> _logger;

        public AccountCreatedEventHandler(ILogger<AccountCreatedEventHandler> logger)
        {
            _logger = logger;
        }

        public async Task HandleAsync(AccountCreatedEvent @event, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Sending notification for account creation: {AccountNumber}", @event.AccountNumber);

                // Simulate notification sending
                await Task.Delay(50, cancellationToken);

                _logger.LogInformation("Notification sent for account {AccountNumber}", @event.AccountNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification for account {AccountNumber}", @event.AccountNumber);
                throw;
            }
        }
    }
}
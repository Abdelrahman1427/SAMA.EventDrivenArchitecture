using SAMA.AccountService.Commands;
using SAMA.EventBus;
using SAMA.SharedKernel.DomainEvents;

namespace SAMA.AccountService.Handlers
{
    public class CreateAccountHandler : IEventHandler<CreateAccountCommand>
    {
        private readonly IEventBus _eventBus;
        private readonly ILogger<CreateAccountHandler> _logger;

        public CreateAccountHandler(IEventBus eventBus, ILogger<CreateAccountHandler> logger)
        {
            _eventBus = eventBus;
            _logger = logger;
        }

        public async Task HandleAsync(CreateAccountCommand command, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Creating account for customer {CustomerId}", command.CustomerId);

                // Simulate account creation logic
                await Task.Delay(100, cancellationToken); // Simulate processing

                var accountCreatedEvent = new AccountCreatedEvent
                {
                    AccountNumber = command.AccountNumber,
                    CustomerId = command.CustomerId,
                    InitialBalance = command.InitialBalance,
                    Currency = command.Currency
                };

                await _eventBus.PublishAsync(accountCreatedEvent, cancellationToken);
                _logger.LogInformation("Account {AccountNumber} created successfully", command.AccountNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create account for customer {CustomerId}", command.CustomerId);
                throw;
            }
        }
    }
}
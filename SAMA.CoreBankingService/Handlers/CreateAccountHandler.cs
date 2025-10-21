using SAMA.AccountService.Commands;
using SAMA.AccountService.Events;
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
                _logger.LogInformation("🏦 Creating account for customer {CustomerId}", command.CustomerId);

                // 1. تحقق من البيانات (بدل ما ننفذ مباشرة)
                await ValidateAccountData(command);

                // 2. ابعت طلب لـ Core Banking
                var coreBankingRequest = new CoreBankingRequestEvent
                {
                    AccountNumber = command.AccountNumber,
                    CustomerId = command.CustomerId,
                    InitialBalance = command.InitialBalance,
                    Currency = command.Currency,
                    RequestType = "ACCOUNT_CREATION"
                };

                await _eventBus.PublishAsync(coreBankingRequest);

                _logger.LogInformation("📤 Account creation request sent to Core Banking for {AccountNumber}", command.AccountNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to process account creation for customer {CustomerId}", command.CustomerId);
                throw;
            }
        }

        private async Task ValidateAccountData(CreateAccountCommand command)
        {
            // تحقق من صحة البيانات
            if (string.IsNullOrEmpty(command.AccountNumber))
                throw new ArgumentException("Account number is required");

            if (string.IsNullOrEmpty(command.CustomerId))
                throw new ArgumentException("Customer ID is required");

            if (command.InitialBalance < 0)
                throw new ArgumentException("Initial balance cannot be negative");

            await Task.CompletedTask;
        }
    }
}
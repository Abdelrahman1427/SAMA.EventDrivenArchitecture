using SAMA.AccountService.Commands;
using SAMA.AccountService.Events;
using SAMA.EventBus;

namespace SAMA.AccountService.Handlers
{
    public class CoreBankingResponseHandler : IEventHandler<CoreBankingProcessedEvent>
    {
        private readonly IEventBus _eventBus;
        private readonly ILogger<CoreBankingResponseHandler> _logger;

        public CoreBankingResponseHandler(IEventBus eventBus, ILogger<CoreBankingResponseHandler> logger)
        {
            _eventBus = eventBus;
            _logger = logger;
        }

        public async Task HandleAsync(CoreBankingProcessedEvent @event, CancellationToken cancellationToken)
        {
            try
            {
                if (@event.Success)
                {
                    _logger.LogInformation("✅ Core Banking successfully processed account {AccountNumber}", @event.AccountNumber);

                    // أنشئ Event نهائي علشان NotificationService
                    var accountCreatedEvent = new AccountCreatedEvent
                    {
                        AccountNumber = @event.AccountNumber,
                        CustomerId = @event.CustomerId,
                        InitialBalance = 0, // Core Banking هيحدد الرصيد
                        Currency = "SAR",
                        CoreBankingReference = @event.CoreBankingReference
                    };

                    await _eventBus.PublishAsync(accountCreatedEvent);
                    _logger.LogInformation("🎉 Account {AccountNumber} fully created in system", @event.AccountNumber);
                }
                else
                {
                    _logger.LogError("❌ Core Banking failed to process account {AccountNumber}: {Message}",
                        @event.AccountNumber, @event.Message);

                    // هنا نقدر نرسل Event للـ Error Handling
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error processing Core Banking response for {AccountNumber}", @event.AccountNumber);
            }
        }
    }
}
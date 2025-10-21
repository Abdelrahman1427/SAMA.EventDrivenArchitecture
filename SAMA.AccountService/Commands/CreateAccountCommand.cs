using SAMA.SharedKernel.DomainEvents;

namespace SAMA.AccountService.Commands
{
    public class CreateAccountCommand : IEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
        public string EventType => nameof(CreateAccountCommand);

        public string AccountNumber { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public decimal InitialBalance { get; set; }
        public string Currency { get; set; } = "SAR";
    }
    public class AccountCreatedEvent : DomainEvent
    {
        public string AccountNumber { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public decimal InitialBalance { get; set; }
        public string Currency { get; set; } = "SAR";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CoreBankingReference { get; set; } = string.Empty; // ⬅️ الجديد
    }
}
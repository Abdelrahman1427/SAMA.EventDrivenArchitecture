using SAMA.SharedKernel.DomainEvents;

namespace SAMA.AccountService.Events
{
    // Event علشان نطلب من Core Banking يعمل حساب
    public class CoreBankingRequestEvent : DomainEvent
    {
        public string AccountNumber { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public decimal InitialBalance { get; set; }
        public string Currency { get; set; } = "SAR";
        public string RequestType { get; set; } = "ACCOUNT_CREATION";
    }

    // Event علشان Core Banking يرد بنجاح المعالجة
    public class CoreBankingProcessedEvent : DomainEvent
    {
        public string AccountNumber { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string CoreBankingReference { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
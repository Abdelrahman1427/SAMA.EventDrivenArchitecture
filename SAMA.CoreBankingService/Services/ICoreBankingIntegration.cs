
namespace SAMA.CoreBankingService.Services
{
    public interface ICoreBankingIntegration
    {
        Task<CoreBankingResponse> CreateAccountAsync(CoreBankingRequest request);
        Task<CoreBankingResponse> GetAccountStatusAsync(string accountNumber);
    }

    public class CoreBankingRequest
    {
        public string AccountNumber { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public decimal InitialBalance { get; set; }
        public string Currency { get; set; } = "SAR";
        public string RequestId { get; set; } = string.Empty;
    }

    public class CoreBankingResponse
    {
        public bool Success { get; set; }
        public string ReferenceNumber { get; set; } = string.Empty;
        public string AccountId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Status { get; set; } = "PROCESSED";
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    }
}
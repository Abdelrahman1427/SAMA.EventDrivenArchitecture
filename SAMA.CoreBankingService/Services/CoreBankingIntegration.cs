using Microsoft.Extensions.Options;

namespace SAMA.CoreBankingService.Services
{
    public class CoreBankingIntegration : ICoreBankingIntegration
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CoreBankingIntegration> _logger;
        private readonly CoreBankingOptions _options;

        public CoreBankingIntegration(
            HttpClient httpClient,
            IOptions<CoreBankingOptions> options,
            ILogger<CoreBankingIntegration> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _options = options.Value;
        }

        public async Task<CoreBankingResponse> CreateAccountAsync(CoreBankingRequest request)
        {
            try
            {
                _logger.LogInformation("🏦 Sending account creation to Core Banking: {AccountNumber}", request.AccountNumber);

                // محاكاة اتصال مع Core Banking System
                await Task.Delay(300); // محاكاة وقت الاستجابة

                // في الواقع، هنا بيكون فيه HTTP call لـ Core Banking API
                // var response = await _httpClient.PostAsJsonAsync(_options.BaseUrl + "/accounts", request);

                // محاكاة response من Core Banking
                var coreBankingResponse = new CoreBankingResponse
                {
                    Success = true,
                    ReferenceNumber = $"CBREF-{DateTime.Now:yyyyMMddHHmmss}",
                    AccountId = Guid.NewGuid().ToString(),
                    Message = "Account created successfully in Core Banking System",
                    Status = "ACTIVE"
                };

                _logger.LogInformation("✅ Core Banking processed account {AccountNumber} successfully", request.AccountNumber);

                return coreBankingResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Core Banking integration failed for {AccountNumber}", request.AccountNumber);

                return new CoreBankingResponse
                {
                    Success = false,
                    Message = $"Core Banking integration failed: {ex.Message}",
                    Status = "FAILED"
                };
            }
        }

        public async Task<CoreBankingResponse> GetAccountStatusAsync(string accountNumber)
        {
            // محاكاة استعلام عن حالة الحساب
            await Task.Delay(100);

            return new CoreBankingResponse
            {
                Success = true,
                Status = "ACTIVE",
                Message = "Account is active in Core Banking"
            };
        }
    }

    public class CoreBankingOptions
    {
        public string BaseUrl { get; set; } = "https://core-banking-api.internal";
        public string ApiKey { get; set; } = string.Empty;
        public int TimeoutSeconds { get; set; } = 30;
    }
}
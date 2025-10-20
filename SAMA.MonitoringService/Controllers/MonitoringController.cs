using Microsoft.AspNetCore.Mvc;
using Confluent.Kafka;
using Confluent.Kafka.Admin;

namespace SAMA.MonitoringService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MonitoringController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<MonitoringController> _logger;

        public MonitoringController(IConfiguration configuration, ILogger<MonitoringController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet("kafka/health")]
        public async Task<IActionResult> CheckKafkaHealth()
        {
            try
            {
                var config = new AdminClientConfig
                {
                    BootstrapServers = _configuration["Kafka:BootstrapServers"]
                };

                using var adminClient = new AdminClientBuilder(config).Build();
                var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10));

                return Ok(new
                {
                    Status = "Healthy",
                    Brokers = metadata.Brokers.Count,
                    Topics = metadata.Topics.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kafka health check failed");
                return StatusCode(503, new { Status = "Unhealthy", Error = ex.Message });
            }
        }
    }
}
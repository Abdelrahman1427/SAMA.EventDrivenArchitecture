using SAMA.EventBus;
using SAMA.EventBus.Kafka;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// إختر واحد فقط - أنصح بـ KafkaEventBus
builder.Services.Configure<KafkaOptions>(builder.Configuration.GetSection("Kafka"));
builder.Services.AddSingleton<IEventBus, KafkaEventBus>();

Console.WriteLine("🚀 Using KafkaEventBus for API Gateway");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Start Event Bus
var eventBus = app.Services.GetRequiredService<IEventBus>();
await eventBus.StartAsync();

Console.WriteLine("✅ SAMA.API.Gateway is running on: https://localhost:7001");

app.Run();
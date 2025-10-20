using SAMA.AccountService.Commands;
using SAMA.AccountService.Handlers;
using SAMA.EventBus;
using SAMA.EventBus.Kafka;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Kafka
builder.Services.Configure<KafkaOptions>(builder.Configuration.GetSection("Kafka"));
builder.Services.AddSingleton<IEventBus, KafkaEventBus>();
// Use InMemoryEventBus for development
builder.Services.AddSingleton<IEventBus, InMemoryEventBus>();
builder.Services.AddScoped<CreateAccountHandler>();

Console.WriteLine("🚀 Using InMemoryEventBus for development");
// Register Handlers
builder.Services.AddScoped<CreateAccountHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Subscribe to events
var eventBus = app.Services.GetRequiredService<IEventBus>();
await eventBus.SubscribeAsync<CreateAccountCommand, CreateAccountHandler>();

await eventBus.StartAsync();
Console.WriteLine("✅ SAMA.AccountService is running on: https://localhost:5111");

app.Run();
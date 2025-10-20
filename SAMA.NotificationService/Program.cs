using SAMA.AccountService.Commands;
using SAMA.EventBus;
using SAMA.EventBus.Kafka;
using SAMA.NotificationService.Handlers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Kafka
builder.Services.Configure<KafkaOptions>(builder.Configuration.GetSection("Kafka"));
builder.Services.AddSingleton<IEventBus, KafkaEventBus>();
// Use InMemoryEventBus for development
builder.Services.AddSingleton<IEventBus, InMemoryEventBus>();
builder.Services.AddScoped<AccountCreatedEventHandler>();

Console.WriteLine("?? Using InMemoryEventBus for development");
// Register Handlers
builder.Services.AddScoped<AccountCreatedEventHandler>();

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
await eventBus.SubscribeAsync<AccountCreatedEvent, AccountCreatedEventHandler>();

await eventBus.StartAsync();
Console.WriteLine("? SAMA.NotificationService is running");
app.Run();
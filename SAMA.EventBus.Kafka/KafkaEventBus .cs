using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SAMA.SharedKernel.DomainEvents;
using System.Collections.Generic;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

namespace SAMA.EventBus.Kafka
{
    public class KafkaEventBus : IEventBus
    {
        private readonly KafkaOptions _options;
        private readonly IProducer<string, string> _producer;
        private readonly IConsumer<string, string> _consumer;
        private readonly ILogger<KafkaEventBus> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<string, Type> _eventTypes = new();
        private readonly Dictionary<Type, List<Type>> _eventHandlers = new();

        public KafkaEventBus(IOptions<KafkaOptions> options, ILogger<KafkaEventBus> logger, IServiceProvider serviceProvider)
        {
            _options = options.Value;
            _logger = logger;
            _serviceProvider = serviceProvider;

            var producerConfig = new ProducerConfig
            {
                BootstrapServers = _options.BootstrapServers,
                Acks = Acks.All
            };

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = _options.BootstrapServers,
                GroupId = _options.GroupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };

            _producer = new ProducerBuilder<string, string>(producerConfig).Build();
            _consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
        }

        public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IEvent
        {
            var topicName = GetTopicName(typeof(TEvent));
            var message = new Message<string, string>
            {
                Key = @event.EventId.ToString(),
                Value = JsonSerializer.Serialize(@event, typeof(TEvent))
            };

            try
            {
                var result = await _producer.ProduceAsync(topicName, message, cancellationToken);
                _logger.LogInformation("Event {EventId} published to {Topic}", @event.EventId, topicName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish event {EventId} to {Topic}", @event.EventId, topicName);
                throw;
            }
        }

        public Task SubscribeAsync<TEvent, THandler>() where TEvent : IEvent where THandler : IEventHandler<TEvent>
        {
            var eventType = typeof(TEvent);
            var handlerType = typeof(THandler);

            if (!_eventHandlers.ContainsKey(eventType))
            {
                _eventHandlers[eventType] = new List<Type>();
            }

            _eventHandlers[eventType].Add(handlerType);
            _eventTypes[GetTopicName(eventType)] = eventType;

            return Task.CompletedTask;
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Starting Kafka Event Bus...");

                await Task.Delay(5000, cancellationToken);

                var topics = _eventTypes.Keys.ToArray();

                if (topics.Length == 0)
                {
                    _logger.LogWarning("No topics to subscribe to");
                    return;
                }

                _logger.LogInformation("Subscribing to topics: {Topics}", string.Join(", ", topics));
                _consumer.Subscribe(topics);

                _ = Task.Run(async () => await ConsumeEventsAsync(cancellationToken), cancellationToken);

                _logger.LogInformation("Kafka Event Bus started successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start Kafka Event Bus");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken = default)
        {
            _producer?.Dispose();
            _consumer?.Dispose();
            return Task.CompletedTask;
        }

        private async Task ConsumeEventsAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(cancellationToken);
                    if (consumeResult?.Message?.Value == null) continue;

                    var topic = consumeResult.Topic;
                    if (_eventTypes.TryGetValue(topic, out var eventType))
                    {
                        var @event = JsonSerializer.Deserialize(consumeResult.Message.Value, eventType) as IEvent;
                        if (@event != null)
                        {
                            await ProcessEventAsync(eventType, @event, cancellationToken);
                        }
                    }

                    _consumer.Commit(consumeResult);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error consuming event from Kafka");
                }
            }
        }

        private async Task ProcessEventAsync(Type eventType, IEvent @event, CancellationToken cancellationToken)
        {
            if (_eventHandlers.TryGetValue(eventType, out var handlerTypes))
            {
                using var scope = _serviceProvider.CreateScope();
                foreach (var handlerType in handlerTypes)
                {
                    var handler = scope.ServiceProvider.GetService(handlerType);
                    if (handler == null) continue;

                    var method = handlerType.GetMethod("HandleAsync");
                    if (method != null)
                    {
                        await (Task)method.Invoke(handler, new object[] { @event, cancellationToken });
                    }
                }
            }
        }

        private static string GetTopicName(Type eventType) => eventType.Name.ToLower();
    }

    public class KafkaOptions
    {
        public string BootstrapServers { get; set; } = "localhost:9092";
        public string GroupId { get; set; } = "sama-group";
        public string AutoOffsetReset { get; set; } = "Earliest";
        public bool EnableAutoCommit { get; set; } = false;
        public bool AllowAutoCreateTopics { get; set; } = true;
        public int MessageTimeoutMs { get; set; } = 5000;
        public int RequestTimeoutMs { get; set; } = 5000;
        public int SessionTimeoutMs { get; set; } = 6000;
    }
}
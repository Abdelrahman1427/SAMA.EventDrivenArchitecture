using Microsoft.Extensions.Logging;
using SAMA.SharedKernel.DomainEvents;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace SAMA.EventBus
{
    public class InMemoryEventBus : IEventBus
    {
        private readonly ILogger<InMemoryEventBus> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly ConcurrentDictionary<Type, List<Type>> _eventHandlers = new();
        private readonly ConcurrentQueue<IEvent> _events = new();
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private bool _isRunning = false;

        public InMemoryEventBus(ILogger<InMemoryEventBus> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IEvent
        {
            _events.Enqueue(@event);
            _logger.LogInformation("Event {EventId} queued for processing", @event.EventId);
            await Task.CompletedTask;
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
            _logger.LogInformation("Handler {HandlerType} subscribed to {EventType}", handlerType.Name, eventType.Name);

            return Task.CompletedTask;
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            if (_isRunning) return;

            _isRunning = true;
            _ = Task.Run(async () => await ProcessEventsAsync(_cancellationTokenSource.Token));
            _logger.LogInformation("InMemoryEventBus started");
        }

        public Task StopAsync(CancellationToken cancellationToken = default)
        {
            _cancellationTokenSource.Cancel();
            _isRunning = false;
            _logger.LogInformation("InMemoryEventBus stopped");
            return Task.CompletedTask;
        }

        private async Task ProcessEventsAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (_events.TryDequeue(out var @event))
                {
                    try
                    {
                        await ProcessEventAsync(@event, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing event {EventId}", @event.EventId);
                    }
                }
                else
                {
                    await Task.Delay(100, cancellationToken);
                }
            }
        }

        private async Task ProcessEventAsync(IEvent @event, CancellationToken cancellationToken)
        {
            var eventType = @event.GetType();
            if (_eventHandlers.TryGetValue(eventType, out var handlerTypes))
            {
                using var scope = _serviceProvider.CreateScope();
                foreach (var handlerType in handlerTypes)
                {
                    try
                    {
                        var handler = scope.ServiceProvider.GetService(handlerType);
                        if (handler == null) continue;

                        var method = handlerType.GetMethod("HandleAsync");
                        if (method != null)
                        {
                            await (Task)method.Invoke(handler, new object[] { @event, cancellationToken });
                            _logger.LogInformation("Event {EventId} handled by {HandlerType}", @event.EventId, handlerType.Name);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error invoking handler {HandlerType} for event {EventId}", handlerType.Name, @event.EventId);
                    }
                }
            }
        }
    }
}
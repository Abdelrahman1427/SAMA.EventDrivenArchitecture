using MediatR;
using SAMA.SharedKernel.DomainEvents;

namespace SAMA.SharedKernel.DomainEvents
{
    public interface IEvent : INotification
    {
        Guid EventId { get; }
        DateTime OccurredOn { get; }
        string EventType { get; }
    }
}

public abstract class DomainEvent : IEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventType => GetType().Name;
}
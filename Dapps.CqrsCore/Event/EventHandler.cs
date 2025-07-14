using System.Threading;
using System.Threading.Tasks;

namespace Dapps.CqrsCore.Event;

/// <summary>
/// Base event handler which register handler for a single event which delivered from event queue
/// </summary>
public abstract class EventHandler<TEvent> : ICqrsEventHandler<TEvent> where TEvent : ICqrsEvent
{
    public abstract Task Handle(TEvent notification, CancellationToken cancellationToken);
}

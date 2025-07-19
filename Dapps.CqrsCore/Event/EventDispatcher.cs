using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Dapps.CqrsCore.Event;

/// <summary>
/// Queue of event message
/// </summary>
public class EventDispatcher : ICqrsEventDispatcher
{
    private readonly IMediator _mediator; //use MediatR to send command to handlers

    public EventDispatcher(IMediator mediator)
    {
        _mediator = mediator;
    }

    public void Publish(ICqrsEvent ev)
    {
        var task = _mediator.Publish(ev);
        task.Wait(); //wait for the task to complete
    }

    public async Task PublishAsync(ICqrsEvent ev, CancellationToken cancellation = default)
    {
        await _mediator.Publish(ev, cancellation);
    }
}

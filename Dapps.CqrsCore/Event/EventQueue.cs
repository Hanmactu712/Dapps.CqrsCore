using System.Threading.Tasks;
using MediatR;

namespace Dapps.CqrsCore.Event;

/// <summary>
/// Queue of event message
/// </summary>
public class EventQueue : ICqrsEventQueue
{
    private readonly IMediator _mediator; //use MediatR to send command to handlers
    public EventQueue(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// publish a event to queue
    /// </summary>
    /// <param name="ev"></param>
    public void Publish(ICqrsEvent ev)
    {
        var task = _mediator.Publish(ev);
        task.Wait(); //wait for the task to complete
    }

    public async Task PublishAsync(ICqrsEvent ev)
    {
        await _mediator.Publish(ev);
    }
}

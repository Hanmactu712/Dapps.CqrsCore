using System.Threading.Tasks;

namespace Dapps.CqrsCore.Event
{
    /// <summary>
    /// Base event handler which register handler for a single event which delivered from event queue
    /// </summary>
    public abstract class EventHandler<TEvent> : IEventHandler<TEvent> where TEvent : IEvent
    {
        //private readonly ILogger _logger;
        protected EventHandler(IEventQueue queue)
        {
            queue.Subscribe<TEvent>(Handle);
        }

        /// <summary>
        /// Handle event logic
        /// </summary>
        /// <param name="message"></param>
        public abstract void Handle(TEvent message);

        public abstract Task HandleAsync(TEvent message);
    }
}

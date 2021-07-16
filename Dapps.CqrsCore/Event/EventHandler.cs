namespace Dapps.CqrsCore.Event
{
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
    }
}

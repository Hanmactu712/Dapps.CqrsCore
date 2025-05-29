using System.Threading.Tasks;

namespace Dapps.CqrsCore.Event
{
    /// <summary>
    /// Interface of event handler
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    public interface IEventHandler<in TEvent> where TEvent : IEvent
    {
        /// <summary>
        /// Handle event logic when received a event from queue
        /// </summary>
        /// <param name="message"></param>
        void Handle(TEvent message);

        /// <summary>
        /// Asynchronously Handle event logic when received a event from queue
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task HandleAsync(TEvent message);
    }
}

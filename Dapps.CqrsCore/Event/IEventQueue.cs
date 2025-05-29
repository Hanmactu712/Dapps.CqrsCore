using System;
using System.Threading.Tasks;

namespace Dapps.CqrsCore.Event
{
    /// <summary>
    /// Interface of Event Queue
    /// </summary>
    public interface IEventQueue
    {
        /// <summary>
        /// publish event to queue
        /// </summary>
        /// <param name="ev"></param>
        void Publish(IEvent ev);

        /// <summary>
        /// Asynchronously subscribe event handler to queue
        /// </summary>
        /// <param name="ev"></param>
        /// <returns></returns>
        Task PublishAsync(IEvent ev);

        /// <summary>
        /// subscribe event handler to queue
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        void Subscribe<T>(Action<T> action) where T : IEvent;

        /// <summary>
        /// Asynchronously subscribe event handler to queue
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        Task SubscribeAsync<T>(Action<T> action) where T : IEvent;
    }
}

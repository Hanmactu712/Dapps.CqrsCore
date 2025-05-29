using Dapps.CqrsCore.Aggregate;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dapps.CqrsCore.Event
{
    /// <summary>
    /// Interface of event repository
    /// </summary>
    public interface IEventRepository
    {

        /// <summary>
        /// Returns the aggregate identified by the specified id.
        /// </summary>
        T Get<T>(Guid id) where T : AggregateRoot;

        /// <summary>
        /// Asynchronously Returns the aggregate identified by the specified id.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<T> GetAsync<T>(Guid id) where T : AggregateRoot;

        /// <summary>
        /// Saves an aggregate.
        /// </summary>
        /// <returns>
        /// Returns the events that are now saved (and ready to be published).
        /// </returns>
        IList<IEvent> Save<T>(T aggregate, int? version = null) where T : AggregateRoot;

        /// <summary>
        /// Asynchronously Saves an aggregate.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="aggregate"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        Task<IList<IEvent>> SaveAsync<T>(T aggregate, int? version = null) where T : AggregateRoot;

        /// <summary>
        /// Copies an aggregate to offline storage and removes it from online logs.
        /// </summary>
        void Box<T>(T aggregate) where T : AggregateRoot;

        /// <summary>
        /// Asynchronously Copies an aggregate to offline storage and removes it from online logs.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="aggregate"></param>
        /// <returns></returns>
        Task BoxAsync<T>(T aggregate) where T : AggregateRoot;

        /// <summary>
        /// Asynchronously Retrieves an aggregate from offline storage and returns only its most recent state.
        /// </summary>
        Task<T> UnboxAsync<T>(Guid aggregate) where T : AggregateRoot;

        /// <summary>
        /// Retrieves an aggregate from offline storage and returns only its most recent state.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="aggregate"></param>
        /// <returns></returns>
        T Unbox<T>(Guid aggregate) where T : AggregateRoot;
    }
}

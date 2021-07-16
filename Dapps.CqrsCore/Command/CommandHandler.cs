using Dapps.CqrsCore.Aggregate;
using Dapps.CqrsCore.Event;
using Microsoft.Extensions.Logging;
using System;

namespace Dapps.CqrsCore.Command
{
    /// <summary>
    /// Base command handlers which register handlers to command queue and commit changes changes to event sourcing as well as public event for subscribers
    /// </summary>
    public abstract class CommandHandler<TCommand> : ICommandHandler<TCommand> where TCommand : ICommand
    {
        private readonly IEventRepository _repository;
        private readonly IEventQueue _eventQueue;

        protected CommandHandler(ICommandQueue queue, IEventRepository eventRepository, IEventQueue eventQueue)
        {
            _eventQueue = eventQueue;
            _repository = eventRepository;
            queue.Subscribe<TCommand>(Handle);
        }

        /// <summary>
        /// Get aggregate from event sourcing
        /// </summary>
        /// <typeparam name="T">aggregate</typeparam>
        /// <param name="id">aggregate id</param>
        /// <returns></returns>
        public virtual T Get<T>(Guid id) where T : AggregateRoot
        {
            return _repository.Get<T>(id);
        }

        /// <summary>
        /// commit all changes of a aggregate to event sourcing
        /// </summary>
        /// <param name="aggregate">aggregate need to save</param>
        public virtual void Commit(AggregateRoot aggregate)
        {
            var changes = _repository.Save(aggregate);
            foreach (var change in changes)
            {
                _eventQueue.Publish(change);
            }
        }

        /// <summary>
        /// Handle command logic
        /// </summary>
        /// <param name="message"></param>
        public abstract void Handle(TCommand message);
    }
}

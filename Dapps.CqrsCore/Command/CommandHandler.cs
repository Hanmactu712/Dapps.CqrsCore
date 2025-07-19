using Dapps.CqrsCore.Aggregate;
using Dapps.CqrsCore.Event;
using System;
using Dapps.CqrsCore.Snapshots;
using System.Threading.Tasks;
using System.Threading;

namespace Dapps.CqrsCore.Command;

/// <summary>
/// Base command handler which register handler for a single command to command queue and commit changes changes to event sourcing as well as public event for subscribers
/// </summary>
public abstract class CommandHandler<TCommand> : ICqrsCommandHandler<TCommand> where TCommand : ICqrsCommand
{
    private readonly ICqrsEventRepository _repository;
    private readonly ICqrsEventDispatcher _eventQueue;

    protected CommandHandler(ICqrsCommandDispatcher queue, ICqrsEventRepository eventRepository, ICqrsEventDispatcher eventQueue, SnapshotRepository snapshotRepository = null)
    {
        _eventQueue = eventQueue ?? throw new ArgumentNullException(nameof(ICqrsCommandDispatcher));

        //using snapshot repository if any, otherwise using normal event repository
        _repository = snapshotRepository ??
                      eventRepository ?? throw new ArgumentNullException(nameof(ICqrsEventRepository));

        if (queue == null)
            throw new ArgumentNullException(nameof(ICqrsEventRepository));
    }

    /// <summary>
    /// Get aggregate from event sourcing
    /// </summary>
    /// <typeparam name="T">aggregate</typeparam>
    /// <param name="id">aggregate id</param>
    /// <returns></returns>
    public virtual T Get<T>(Guid id) where T : CqrsAggregateRoot
    {
        return _repository.Get<T>(id);
    }

    /// <summary>
    /// Get aggregate from event sourcing asynchronously
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="id"></param>
    /// <returns></returns>
    public virtual async Task<T> GetAsync<T>(Guid id, CancellationToken cancellation = default) where T : CqrsAggregateRoot
    {
        return await _repository.GetAsync<T>(id, cancellation);
    }

    /// <summary>
    /// commit all changes of a aggregate to event sourcing
    /// </summary>
    /// <param name="aggregate">aggregate need to save</param>
    public virtual void Commit(CqrsAggregateRoot aggregate)
    {
        var changes = _repository.Save(aggregate);
        foreach (var change in changes)
        {
            _eventQueue.Publish(change);
        }
    }

    /// <summary>
    /// commit all changes of a aggregate to event sourcing asynchronously
    /// </summary>
    /// <param name="aggregate"></param>
    /// <returns></returns>
    public virtual async Task CommitAsync(CqrsAggregateRoot aggregate, CancellationToken cancellation = default)
    {
        var changes = await _repository.SaveAsync(aggregate, cancellation: cancellation);

        foreach (var change in changes)
        {
            await _eventQueue.PublishAsync(change, cancellation);
        }
    }

    /// <summary>
    /// Handle command logic
    /// </summary>
    /// <param name="message"></param>        
    public abstract Task Handle(TCommand request, CancellationToken cancellationToken);

    /// <summary>
    /// Get the event repository used to persist events and aggregates
    /// </summary>
    protected ICqrsEventRepository EventRepository => _repository;
}
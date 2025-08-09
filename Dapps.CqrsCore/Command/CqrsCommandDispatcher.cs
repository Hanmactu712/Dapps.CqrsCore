using Dapps.CqrsCore.Exceptions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Dapps.CqrsCore.Command;

/// <summary>
/// Default implementation of <see cref="ICqrsCommandDispatcher"/> interface. Which is handlers for commands in memory and use <see cref="ICqrsCommandStore"/> to store commands.
/// </summary>
public class CqrsCommandDispatcher : ICqrsCommandDispatcher
{
    private readonly ICqrsCommandStore _store;
    private readonly bool _saveAll;
    private readonly IMediator _mediator; //use MediatR to send command to handlers

    public CqrsCommandDispatcher(ICqrsCommandStore store, CommandStoreOptions option, IMediator mediator)
    {
        _store = store ?? throw new ArgumentNullException(nameof(ICqrsCommandStore));
        if (option == null)
        {
            throw new ArgumentNullException(nameof(CommandStoreOptions));
        }
        _saveAll = option.SaveAll;

        _mediator = mediator;
    }

    /// <summary>
    /// cancel a schedule command
    /// </summary>
    /// <param name="commandId"></param>
    public void Cancel(Guid commandId)
    {
        var serializedCommand = _store.Get(commandId);
        if (serializedCommand != null)
        {
            serializedCommand.SendCancelled = DateTimeOffset.UtcNow;
            serializedCommand.SendStatus = CommandStatus.Cancelled;
            _store.Save(serializedCommand, false);
        }
    }

    public async Task CancelAsync(Guid commandId, CancellationToken cancellation = default)
    {
        var serializedCommand = await _store.GetAsync(commandId, cancellation);
        if (serializedCommand != null)
        {
            serializedCommand.SendCancelled = DateTimeOffset.UtcNow;
            serializedCommand.SendStatus = CommandStatus.Cancelled;
            await _store.SaveAsync(serializedCommand, false, cancellation);
        }
    }

    /// <summary>
    /// complete a scheduled command
    /// </summary>
    /// <param name="commandId"></param>
    public void Complete(Guid commandId)
    {
        var serializedCommand = _store.Get(commandId);
        if (serializedCommand != null)
        {
            serializedCommand.SendCompleted = DateTimeOffset.UtcNow;
            serializedCommand.SendStatus = CommandStatus.Completed;
            _store.Save(serializedCommand, false);
        }
    }

    public async Task CompleteAsync(Guid commandId, CancellationToken cancellation = default)
    {
        var serializedCommand = await _store.GetAsync(commandId, cancellation);
        if (serializedCommand != null)
        {
            serializedCommand.SendCompleted = DateTimeOffset.UtcNow;
            serializedCommand.SendStatus = CommandStatus.Completed;
            await _store.SaveAsync(serializedCommand, false, cancellation);
        }
    }

    /// <summary>
    /// schedule a command 
    /// </summary>
    /// <param name="command"></param>
    /// <param name="at"></param>
    public void Schedule(ICqrsCommand command, DateTimeOffset at)
    {
        var serializedCommand = _store.Serialize(command);

        if (serializedCommand == null)
            throw new ArgumentNullException(nameof(CommandStoreOptions));

        serializedCommand.SendScheduled = at;
        serializedCommand.SendStatus = CommandStatus.Scheduled;
        _store.Save(serializedCommand, true);
    }

    public async Task ScheduleAsync(ICqrsCommand command, DateTimeOffset at, CancellationToken cancellation = default)
    {
        var serializedCommand = _store.Serialize(command);

        if (serializedCommand == null)
            throw new ArgumentNullException(nameof(CommandStoreOptions));

        serializedCommand.SendScheduled = at;
        serializedCommand.SendStatus = CommandStatus.Scheduled;
        await _store.SaveAsync(serializedCommand, true, cancellation);
    }

    /// <summary>
    /// execute command synchronously
    /// </summary>
    /// <param name="command"></param>
    public void Send(ICqrsCommand command)
    {
        SerializedCommand serialized = null;

        if (_saveAll)
        {
            serialized = _store.Serialize(command);
            serialized.SendStarted = DateTimeOffset.UtcNow;
        }

        var task = ExecuteAsync(command);
        task.Wait();

        if (_saveAll)
        {
            if (serialized != null)
            {
                serialized.SendCompleted = DateTimeOffset.UtcNow;
                serialized.SendStatus = CommandStatus.Completed;
                _store.Save(serialized, true);
            }
        }
    }

    public async Task SendAsync(ICqrsCommand command, CancellationToken cancellation = default)
    {
        SerializedCommand serialized = null;

        if (_saveAll)
        {
            serialized = _store.Serialize(command);
            serialized.SendStarted = DateTimeOffset.UtcNow;
        }

        await ExecuteAsync(command, cancellation);

        if (_saveAll)
        {
            if (serialized != null)
            {
                serialized.SendCompleted = DateTimeOffset.UtcNow;
                serialized.SendStatus = CommandStatus.Completed;
                await _store.SaveAsync(serialized, true, cancellation);
            }
        }
    }

    private async Task ExecuteAsync(ICqrsCommand command, CancellationToken cancellation = default)
    {
        await _mediator.Send(command, cancellation);
    }

    /// <summary>
    /// Start a scheduled command
    /// </summary>
    /// <param name="commandId"></param>
    public void Start(Guid commandId)
    {
        Execute(_store.Get(commandId));
    }

    public async Task StartAsync(Guid commandId, CancellationToken cancellation = default)
    {
        await ExecuteAsync(await _store.GetAsync(commandId, cancellation), cancellation);
    }

    /// <summary>
    /// execute a command asynchronously
    /// </summary>
    /// <param name="serializedCommand"></param>
    private void Execute(SerializedCommand serializedCommand)
    {
        if (serializedCommand == null)
            throw new ArgumentNullException(nameof(SerializedCommand));

        serializedCommand.SendStarted = DateTimeOffset.UtcNow;
        serializedCommand.SendStatus = CommandStatus.Started;
        _store.Save(serializedCommand, false);

        var task = ExecuteAsync(serializedCommand.Deserialize(_store.Serializer));
        task.Wait();

        serializedCommand.SendCompleted = DateTimeOffset.UtcNow;
        serializedCommand.SendStatus = CommandStatus.Completed;
        _store.Save(serializedCommand, false);
    }

    private async Task ExecuteAsync(SerializedCommand serializedCommand, CancellationToken cancellation = default)
    {
        if (serializedCommand == null)
            throw new ArgumentNullException(nameof(SerializedCommand));

        serializedCommand.SendStarted = DateTimeOffset.UtcNow;
        serializedCommand.SendStatus = CommandStatus.Started;
        await _store.SaveAsync(serializedCommand, false, cancellation);

        await ExecuteAsync(serializedCommand.Deserialize(_store.Serializer), cancellation);

        serializedCommand.SendCompleted = DateTimeOffset.UtcNow;
        serializedCommand.SendStatus = CommandStatus.Completed;
        await _store.SaveAsync(serializedCommand, false, cancellation);
    }

    /// <summary>
    /// Ping to scheduled command queue to execute all the expired command
    /// </summary>
    public void Ping()
    {
        var commands = _store.GetExpired(DateTimeOffset.UtcNow);
        foreach (var command in commands)
        {
            var task = ExecuteAsync(command);
            task.Wait();
        }
    }

    public async Task PingAsync(CancellationToken cancellation = default)
    {
        var commands = await _store.GetExpiredAsync(DateTimeOffset.UtcNow, cancellation);
        foreach (var command in commands)
            await ExecuteAsync(command, cancellation);

    }
}

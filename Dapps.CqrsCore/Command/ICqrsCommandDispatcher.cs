using System;
using System.Threading.Tasks;

namespace Dapps.CqrsCore.Command;

/// <summary>
/// Interface of a command queue which carries all the command and deliver command to subscribers
/// </summary>
public interface ICqrsCommandDispatcher
{
    /// <summary>
    /// Send a command as synchronous
    /// </summary>
    /// <param name="command"></param>
    void Send(ICqrsCommand command);

    /// <summary>
    /// Send a command asynchronously
    /// </summary>
    /// <param name="command"></param>
    Task SendAsync(ICqrsCommand command);

    /// <summary>
    /// Send a command as assynchronous
    /// </summary>
    /// <param name="command"></param>
    /// <param name="at"></param>
    void Schedule(ICqrsCommand command, DateTimeOffset at);

    /// <summary>
    /// Schedule a command to be sent asynchronously at a specific time
    /// </summary>
    /// <param name="command"></param>
    /// <param name="at"></param>
    Task ScheduleAsync(ICqrsCommand command, DateTimeOffset at);

    /// <summary>
    /// Start a scheduled command
    /// </summary>
    /// <param name="commandID"></param>
    void Start(Guid commandID);

    /// <summary>
    /// Start a scheduled command asynchronously
    /// </summary>
    /// <param name="commandID"></param>
    Task StartAsync(Guid commandID);

    /// <summary>
    /// Cancel a scheduled command
    /// </summary>
    /// <param name="commandID"></param>
    void Cancel(Guid commandID);

    /// <summary>
    /// Cancel a scheduled command asynchronously
    /// </summary>
    /// <param name="commandID"></param>
    Task CancelAsync(Guid commandID);

    /// <summary>
    /// Complete a scheduled command
    /// </summary>
    /// <param name="commandID"></param>
    void Complete(Guid commandID);

    /// <summary>
    /// Complete a scheduled command asynchronously
    /// </summary>
    /// <param name="commandID"></param>
    Task CompleteAsync(Guid commandID);

    /// <summary>
    /// Ping to the command store to awake all scheduled command haven't been handled.
    /// </summary>
    void Ping();

    /// <summary>
    /// Ping to the command store to awake all scheduled command haven't been handled asynchronously.
    /// </summary>
    /// <param name="commandID"></param>
    Task PingAsync();
}

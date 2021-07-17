using System;

namespace Dapps.CqrsCore.Command
{
    /// <summary>
    /// Interface of a command queue which carries all the command and deliver command to subscribers
    /// </summary>
    public interface ICommandQueue
    {
        /// <summary>
        /// subscribe a handler for specific commnad
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        void Subscribe<T>(Action<T> action) where T : ICommand;

        /// <summary>
        /// Send a command as synchronous
        /// </summary>
        /// <param name="command"></param>
        void Send(ICommand command);

        /// <summary>
        /// Send a command as assynchronous
        /// </summary>
        /// <param name="command"></param>
        /// <param name="at"></param>
        void Schedule(ICommand command, DateTimeOffset at);

        /// <summary>
        /// Start a scheduled command
        /// </summary>
        /// <param name="commandID"></param>
        void Start(Guid commandID);

        /// <summary>
        /// Cancel a scheduled command
        /// </summary>
        /// <param name="commandID"></param>
        void Cancel(Guid commandID);

        /// <summary>
        /// Complete a scheduled command
        /// </summary>
        /// <param name="commandID"></param>
        void Complete(Guid commandID);

        /// <summary>
        /// Ping to the command store to awake all scheduled command haven't been handled.
        /// </summary>
        void Ping();
    }
}

using Dapps.CqrsCore.Exceptions;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dapps.CqrsCore.Command
{
    public class CommandQueue : ICommandQueue
    {
        private readonly Dictionary<string, Action<ICommand>> _subscribers;
        private readonly ICommandStore _store;
        private readonly bool _saveAll;

        public CommandQueue(ICommandStore store, IConfiguration configuration)
        {
            _store = store;

            var saveAll = Convert.ToBoolean(configuration.GetSection("CoreSettings:SaveAll").Value);

            _saveAll = saveAll;

            _subscribers = new Dictionary<string, Action<ICommand>>();

        }

        /// <summary>
        /// cancel a schedule command
        /// </summary>
        /// <param name="commandID"></param>
        public void Cancel(Guid commandID)
        {
            var serializedCommand = _store.Get(commandID);
            if (serializedCommand != null)
            {
                serializedCommand.SendCancelled = DateTimeOffset.UtcNow;
                serializedCommand.SendStatus = CommandStatus.Cancelled;
                _store.Save(serializedCommand, false);
            }
        }

        /// <summary>
        /// complete a scheduled command
        /// </summary>
        /// <param name="commandID"></param>
        public void Complete(Guid commandID)
        {
            var serializedCommand = _store.Get(commandID);
            if (serializedCommand != null)
            {
                serializedCommand.SendCompleted = DateTimeOffset.UtcNow;
                serializedCommand.SendStatus = CommandStatus.Completed;
                _store.Save(serializedCommand, false);
            }
        }

        /// <summary>
        /// schedule a command 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="at"></param>
        public void Schedule(ICommand command, DateTimeOffset at)
        {
            var serializedCommand = _store.Serialize(command);
            serializedCommand.SendScheduled = at;
            serializedCommand.SendStatus = CommandStatus.Scheduled;
            _store.Save(serializedCommand, true);
        }

        /// <summary>
        /// execute command synchronously
        /// </summary>
        /// <param name="command"></param>
        public void Send(ICommand command)
        {
            SerializedCommand serialized = null;

            if (_saveAll)
            {
                serialized = _store.Serialize(command);
                serialized.SendStarted = DateTimeOffset.UtcNow;
            }

            Execute(command, command.GetType().AssemblyQualifiedName);

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

        /// <summary>
        /// execute command synchronously
        /// </summary>
        /// <param name="command"></param>
        /// <param name="className"></param>
        private void Execute(ICommand command, string className)
        {
            if (_subscribers.ContainsKey(className))
            {
                var action = _subscribers[className];
                action.Invoke(command);
            }
            else
            {
                throw new UnhandledCommandException(className);
            }
        }

        /// <summary>
        /// Start a scheduled command
        /// </summary>
        /// <param name="commandID"></param>
        public void Start(Guid commandID)
        {
            Execute(_store.Get(commandID));
        }

        /// <summary>
        /// execute a command asynchronously
        /// </summary>
        /// <param name="serializedCommand"></param>
        private void Execute(SerializedCommand serializedCommand)
        {
            serializedCommand.SendStarted = DateTimeOffset.UtcNow;
            serializedCommand.SendStatus = CommandStatus.Started;
            _store.Save(serializedCommand, false);

            Execute(serializedCommand.Deserialize(_store.Serializer), serializedCommand.Class);

            serializedCommand.SendCompleted = DateTimeOffset.UtcNow;
            serializedCommand.SendStatus = CommandStatus.Completed;
            _store.Save(serializedCommand, false);
        }

        /// <summary>
        /// register a command handler for specific command.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        public void Subscribe<T>(Action<T> action) where T : ICommand
        {
            var name = typeof(T).AssemblyQualifiedName;
            if (_subscribers.Any(s => s.Key.Equals(name)))
                throw new AmbiguousCommandHandlerException(name);

            if (name != null)
                _subscribers.Add(name, (command) => action((T)command));
        }

        /// <summary>
        /// Ping to scheduled command queue to execute all the expired command
        /// </summary>
        public void Ping()
        {
            var commands = _store.GetExpired(DateTimeOffset.UtcNow);
            foreach (var command in commands)
                Execute(command);

        }
    }
}

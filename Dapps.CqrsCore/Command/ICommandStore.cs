using Dapps.CqrsCore.Utilities;
using System;
using System.Collections.Generic;

namespace Dapps.CqrsCore.Command
{
    public interface ICommandStore
    {
        ISerializer Serializer { get; }

        /// <summary>
        /// Check if a command is exists
        /// </summary>
        /// <param name="commandID"></param>
        /// <returns></returns>
        bool Exists(Guid commandID);


        /// <summary>
        /// Get a serialized command
        /// </summary>
        /// <param name="commandID"></param>
        /// <returns></returns>
        SerializedCommand Get(Guid commandID);

        /// <summary>
        /// Get all unstarted commands that are scheduled to send now
        /// </summary>
        /// <param name="at"></param>
        /// <returns></returns>
        IEnumerable<SerializedCommand> GetExpired(DateTimeOffset at);

        /// <summary>
        /// save command to store
        /// </summary>
        /// <param name="command"></param>
        /// <param name="isNew"></param>
        void Save(SerializedCommand command, bool isNew);

        /// <summary>
        /// Serialize a command
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        SerializedCommand Serialize(ICommand command);
    }
}

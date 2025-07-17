using Dapps.CqrsCore.Utilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dapps.CqrsCore.Command;

/// <summary>
/// Interface of a command store which have the responsibility to persist command
/// </summary>
public interface ICqrsCommandStore
{
    /// <summary>
    /// Serializer using to serialize command & event to persist
    /// </summary>
    ISerializer Serializer { get; }

    /// <summary>
    /// Check if a command is exists
    /// </summary>
    /// <param name="commandID"></param>
    /// <returns></returns>
    bool Exists(Guid commandID);

    Task<bool> ExistsAsync(Guid commandID);

    /// <summary>
    /// Get a serialized command
    /// </summary>
    /// <param name="commandID"></param>
    /// <returns></returns>
    SerializedCommand Get(Guid commandID);

    Task<SerializedCommand> GetAsync(Guid commandID);

    /// <summary>
    /// Get a serialized commands based on aggregateId
    /// </summary>
    /// <param name="aggregateId"></param>
    /// <returns></returns>
    IEnumerable<SerializedCommand> GetByAggregateId(Guid aggregateId);

    Task<IEnumerable<SerializedCommand>> GetByAggregateIdAsync(Guid aggregateId);

    /// <summary>
    /// Get all un-started commands that are scheduled to send now
    /// </summary>
    /// <param name="at"></param>
    /// <returns></returns>
    IEnumerable<SerializedCommand> GetExpired(DateTimeOffset at);

    Task<IEnumerable<SerializedCommand>> GetExpiredAsync(DateTimeOffset at);

    /// <summary>
    /// save command to store
    /// </summary>
    /// <param name="command"></param>
    /// <param name="isNew"></param>
    void Save(SerializedCommand command, bool isNew);

    Task SaveAsync(SerializedCommand command, bool isNew);

    /// <summary>
    /// Serialize a command
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    SerializedCommand Serialize(ICqrsCommand command);

    /// <summary>
    /// Copies all command for an aggregate to offline storage and removes it from online logs.
    /// </summary>
    /// <remarks>
    /// Someone who is a purist with regard to event sourcing will red-flag this function and say the event stream 
    /// for an aggregate should never be altered or removed. However, we have two scenarios in which this is a non-
    /// negotiable business requirement. First, when a customer does not renew their contract with our business, we
    /// have a contractual obligation to remove all the customer's data from our systems. Second, we frequently run
    /// test-cases to confirm system functions are operating correctly; this data is temporary by definition, and 
    /// we do not want to permanently store the event streams for test aggregates.
    /// </remarks>
    void Box(Guid aggregateId);

    Task BoxAsync(Guid aggregateId);

    /// <summary>
    /// Retrieve command from file and insert back to command store
    /// </summary>
    /// <param name="aggregateId"></param>
    /// <returns></returns>
    IEnumerable<SerializedCommand> UnBox(Guid aggregateId);

    Task<IEnumerable<SerializedCommand>> UnBoxAsync(Guid aggregateId);
}

using Dapps.CqrsCore.Utilities;
using System;

namespace Dapps.CqrsCore.Command;

public static class CommandExtensions
{
    /// <summary>
    /// Convert serialized command entity from database to the operation command
    /// </summary>
    /// <param name="serializedCommand"></param>
    /// <param name="serializer"></param>
    /// <returns></returns>
    public static ICqrsCommand Deserialize(this SerializedCommand serializedCommand, ICqrsSerializer serializer)
    {
        if (serializedCommand == null)
            throw new ArgumentNullException(nameof(SerializedCommand));

        if (serializer == null)
            throw new ArgumentNullException(nameof(ICqrsSerializer));

        var data = serializer.Deserialize<ICqrsCommand>(serializedCommand.Data, Type.GetType(serializedCommand.Class));

        if (data == null) return null;

        data.Id = serializedCommand.Id;
        //data.UserId = serializedCommand.UserId;
        data.Version = serializedCommand.Version;
        return data;
    }

    /// <summary>
    /// Convert command message to serialized command entity to persist in the database
    /// </summary>
    /// <param name="command"></param>
    /// <param name="serializer"></param>
    /// <param name="aggregateId"></param>
    /// <param name="version"></param>
    /// <returns></returns>
    public static SerializedCommand Serialize(this ICqrsCommand command, ICqrsSerializer serializer, int? version)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(ICqrsCommand));

        if (serializer == null)
            throw new ArgumentNullException(nameof(ICqrsSerializer));

        var data = serializer.Serialize(command, new[] { "Version", "SendScheduled", "SendStarted", "SendCompleted", "SendCancelled" });

        var serialized = new SerializedCommand
        {
            Id = command.Id,
            AggregateId = command.AggregateId,
            Version = version,
            Class = command.GetType().AssemblyQualifiedName,
            Type = command.GetType().Name,
            Data = data,
        };

        if (serialized.Class != null && serialized.Class.Length > 200)
            throw new OverflowException($"The assembly-qualified name for this command ({serialized.Class}) exceeds the maximum character limit (200).");

        if (serialized.Type.Length > 100)
            throw new OverflowException($"The type name for this command ({serialized.Type}) exceeds the maximum character limit (200).");

        //if ((serialized.SendStatus?.Length ?? 0) > 20)
        //    throw new OverflowException($"The send status ({serialized.SendStatus}) exceeds the maximum character limit (20).");

        return serialized;
    }
}

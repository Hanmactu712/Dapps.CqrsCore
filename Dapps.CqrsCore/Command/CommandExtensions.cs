using Dapps.CqrsCore.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dapps.CqrsCore.Command
{
    public static class CommandExtensions
    {
        public static ICommand Deserialize(this SerializedCommand serializedCommand, ISerializer serializer)
        {
            var data = serializer.Deserialize<ICommand>(serializedCommand.Data, Type.GetType(serializedCommand.Class));

            data.Id = serializedCommand.Id;
            //data.UserId = serializedCommand.UserId;
            data.Version = serializedCommand.Version;

            return data;
        }

        public static SerializedCommand Serialize(this ICommand command, ISerializer serializer, Guid aggregateId, int? version)
        {
            var data = serializer.Serialize(command, new[] { "Version", "SendScheduled", "SendStarted", "SendCompleted", "SendCancelled" });

            var serialized = new SerializedCommand
            {
                Id = command.Id,
                AggregateId = aggregateId,
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
}

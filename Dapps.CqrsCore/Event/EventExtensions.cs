using Dapps.CqrsCore.Utilities;
using System;

namespace Dapps.CqrsCore.Event;

public static class EventExtensions
{
    /// <summary>
    /// Returns a deserialized event.
    /// </summary>
    public static ICqrsEvent Deserialize(this SerializedEvent x, ISerializer serializer)
    {
        var data = serializer.Deserialize<ICqrsEvent>(x.Data, Type.GetType(x.Class));

        data.AggregateId = x.AggregateId;
        data.Version = x.Version;
        data.Time = x.Time;
        data.ReferenceId = x.ReferenceId;
        //data.UserId = x.UserID;

        return data;
    }

    /// <summary>
    /// Returns a serialized event.
    /// </summary>
    public static SerializedEvent Serialize(this ICqrsEvent ev, ISerializer serializer, Guid aggregateId, int version)
    {
        var data = serializer.Serialize(ev, new[] { "AggregateId", "Version", "Time", "ReferenceId" });

        var serialized = new SerializedEvent
        {
            AggregateId = aggregateId,
            Version = version,
            Time = ev.Time,
            Class = ev.GetType().AssemblyQualifiedName,
            Type = ev.GetType().FullName,
            Data = data,
            ReferenceId = ev.ReferenceId
        };

        //ev.UserId = serialized.UserID;

        return serialized;
    }
}

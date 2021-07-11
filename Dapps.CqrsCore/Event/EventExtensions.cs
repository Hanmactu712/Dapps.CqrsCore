using Dapps.CqrsCore.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dapps.CqrsCore.Event
{
    public static class EventExtensions
    {
        /// <summary>
        /// Returns a deserialized event.
        /// </summary>
        public static IEvent Deserialize(this SerializedEvent x, ISerializer serializer)
        {
            var data = serializer.Deserialize<IEvent>(x.Data, Type.GetType(x.Class));

            data.Id = x.AggregateID;
            data.Version = x.Version;
            data.Time = x.Time;
            data.UserId = x.UserID;

            return data;
        }

        /// <summary>
        /// Returns a serialized event.
        /// </summary>
        public static SerializedEvent Serialize(this IEvent ev, ISerializer serializer, Guid aggregateId, int version, Guid user)
        {
            var data = serializer.Serialize(ev, new[] { "AggregateID", "Version", "Time", "UserID" });

            var serialized = new SerializedEvent
            {
                AggregateID = aggregateId,
                Version = version,

                Time = ev.Time,
                Class = ev.GetType().AssemblyQualifiedName,
                Type = ev.GetType().Name,
                Data = data,
                UserID = Guid.Empty == ev.UserId ? user : ev.UserId
            };

            ev.UserId = serialized.UserID;

            return serialized;
        }
    }
}

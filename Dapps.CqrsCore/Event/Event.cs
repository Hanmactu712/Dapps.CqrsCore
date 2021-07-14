using System;

namespace Dapps.CqrsCore.Event
{
    public abstract class Event : IEvent
    {
        public Guid AggregateId { get; set; }
        public int Version { get; set; }
        public Guid UserId { get; set; }
        public Guid TenantId { get; set; }
        public string EventType { get; set; }
        public string Class { get; set; }
        public string Data { get; set; }
        public DateTimeOffset Time { get; set; }
        public Guid ReferenceId { get; set; }

        protected Event()
        {
            Time = DateTimeOffset.UtcNow;
        }
    }
}

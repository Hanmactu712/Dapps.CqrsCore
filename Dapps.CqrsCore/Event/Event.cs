using System;
using System.Collections.Generic;
using System.Text;

namespace Dapps.CqrsCore.Event
{
    public abstract class Event : IEvent
    {
        public Guid Id { get; set; }
        public int Version { get; set; }
        public Guid UserId { get; set; }
        public Guid TenantId { get; set; }
        public string EventType { get; set; }
        public string Class { get; set; }
        public string Data { get; set; }
        public DateTimeOffset Time { get; set; }

        protected Event()
        {
            //ID = Guid.NewGuid();
            Time = DateTimeOffset.UtcNow;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Dapps.CqrsCore.Event
{
    public class SerializedEvent
    {
        //public Guid ID { get; set; }
        public Guid AggregateID { get; set; }
        public int Version { get; set; }
        public Guid UserID { get; set; }

        public string Class { get; set; }
        public string Type { get; set; }
        public string Data { get; set; }
        public DateTimeOffset Time { get; set; }
    }
}

using System;

namespace Dapps.CqrsCore.Command
{
    public class SerializedCommand : ICommand
    {
        public Guid Id { get; set; }
        public Guid AggregateId { get; set; }
        public int? Version { get; set; }
        public string Class { get; set; }
        public string Type { get; set; }
        public string Data { get; set; }
        public DateTimeOffset? SendScheduled { get; set; }
        public DateTimeOffset? SendStarted { get; set; }
        public DateTimeOffset? SendCompleted { get; set; }
        public DateTimeOffset? SendCancelled { get; set; }
        public CommandStatus SendStatus { get; set; }
        public string SendError { get; set; }
    }
}

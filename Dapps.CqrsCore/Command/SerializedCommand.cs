using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dapps.CqrsCore.Command
{
    public class SerializedCommand : ICommand
    {
        [NotMapped]
        public Guid Id { get; set; }
        public int? Version { get; set; }

        [Key]
        public Guid CommandId { get; set; }
        public Guid UserId { get; set; }

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

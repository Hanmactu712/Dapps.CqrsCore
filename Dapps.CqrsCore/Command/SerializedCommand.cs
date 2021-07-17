using System;

namespace Dapps.CqrsCore.Command
{
    /// <summary>
    /// the entity format of command which will be persisted in the event store
    /// </summary>
    public class SerializedCommand : ICommand
    {
        /// <summary>
        /// unique id
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// aggregate unique id which command is cater for
        /// </summary>
        public Guid AggregateId { get; set; }
        /// <summary>
        /// version of the command, nullable
        /// </summary>
        public int? Version { get; set; }
        /// <summary>
        /// class name of the command
        /// </summary>
        public string Class { get; set; }
        /// <summary>
        /// type of the command
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// Data which command carries
        /// </summary>
        public string Data { get; set; }
        /// <summary>
        /// schedule time of the command
        /// </summary>
        public DateTimeOffset? SendScheduled { get; set; }
        /// <summary>
        /// started time of the command
        /// </summary>
        public DateTimeOffset? SendStarted { get; set; }
        /// <summary>
        /// completion time of the command
        /// </summary>
        public DateTimeOffset? SendCompleted { get; set; }
        /// <summary>
        /// cancellation time of the command
        /// </summary>
        public DateTimeOffset? SendCancelled { get; set; }
        /// <summary>
        /// sending status of the command
        /// </summary>
        public CommandStatus SendStatus { get; set; }
        /// <summary>
        /// error message if any when sending command
        /// </summary>
        public string SendError { get; set; }
    }
}

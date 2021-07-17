using System;

namespace Dapps.CqrsCore.Command
{
    /// <summary>
    /// Interface of the command
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// unique ID for command
        /// </summary>
        Guid Id { get; set; }
        /// <summary>
        /// Aggregate AggregateId which command is carried for
        /// </summary>
        Guid AggregateId { get; set; }
        /// <summary>
        /// Aggregate version
        /// </summary>
        int? Version { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Dapps.CqrsCore.Command
{
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

using System;

namespace Dapps.CqrsCore.Command
{
    /// <summary>
    /// Base class for command
    /// </summary>
    public abstract class Command : ICommand
    {
        /// <summary>
        /// Command unique Id
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Aggregate unique Id
        /// </summary>
        public Guid AggregateId { get; set; }
        /// <summary>
        /// Command version
        /// </summary>
        public int? Version { get; set; }
        /// <summary>
        /// Tenant Id
        /// </summary>
        public Guid TenantId { get; set; }
        /// <summary>
        /// User id who raise the command
        /// </summary>
        public Guid UserId { get; set; }
        /// <summary>
        /// Time the command is raised
        /// </summary>
        public DateTimeOffset Time { get; set; }

        protected Command()
        {
            Id = Guid.NewGuid();
            Time = DateTime.UtcNow;
        }
    }
}

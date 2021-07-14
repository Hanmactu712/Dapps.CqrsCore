using System;

namespace Dapps.CqrsCore.Command
{
    public abstract class Command : ICommand
    {
        public Guid Id { get; set; }
        public Guid AggregateId { get; set; }
        public int? Version { get; set; }
        public Guid TenantId { get; set; }
        public Guid UserId { get; set; }
        public DateTimeOffset Time { get; set; }

        protected Command()
        {
            Id = Guid.NewGuid();
            Time = DateTime.UtcNow;
        }
    }
}

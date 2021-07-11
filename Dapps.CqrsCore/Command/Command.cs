using System;

namespace Dapps.CqrsCore.Command
{
    public abstract class Command : ICommand
    {
        public Guid CommandId { get; set; }
        public Guid Id { get; set; }
        public int? Version { get; set; }
        public Guid UserId { get; set; }
        public Guid TenantId { get; set; }
        public DateTimeOffset Time { get; set; }

        protected Command()
        {
            CommandId = Guid.NewGuid();
            Time = DateTime.UtcNow;
        }
    }
}

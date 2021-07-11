using Dapps.CqrsCore.Aggregate;
using Dapps.CqrsCore.Command;
using Dapps.CqrsCore.Event;
using Dapps.CqrsCore.Persistence.Configuration;
using Dapps.CqrsCore.Snapshots;
using Microsoft.EntityFrameworkCore;

namespace Dapps.CqrsCore.Persistence.Store
{
    public class EventSourcingDBContext : DbContext
    {

        public EventSourcingDBContext(DbContextOptions<EventSourcingDBContext> options) : base(options)
        {

        }
        public DbSet<SerializedAggregate> Aggregates { get; set; }
        public DbSet<SerializedCommand> Commands { get; set; }
        public DbSet<SerializedEvent> Events { get; set; }
        public DbSet<Snapshot> Snapshots { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new CommandConfiguration(""));
            modelBuilder.ApplyConfiguration(new EventConfiguration(""));
            modelBuilder.ApplyConfiguration(new AggregateConfiguration(""));
            modelBuilder.ApplyConfiguration(new SnapshotConfiguration(""));
        }
    }

}

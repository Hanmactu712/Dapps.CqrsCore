using Dapps.CqrsCore.Aggregate;
using Dapps.CqrsCore.Event;
using Dapps.CqrsCore.Persistence.Configuration;
using Dapps.CqrsCore.Persistence.Store;
using Microsoft.EntityFrameworkCore;

namespace Dapps.CqrsSample.EventSourcing
{
    public class EventDbContext : DbContext, IEventDbContext
    {
        public EventDbContext(DbContextOptions<EventDbContext> options) : base(options)
        {

        }
        public DbSet<SerializedAggregate> Aggregates { get; set; }
        public DbSet<SerializedEvent> Events { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            
            modelBuilder.ApplyConfiguration(new EventConfiguration(""));
            modelBuilder.ApplyConfiguration(new AggregateConfiguration(""));
            
        }
    }
}

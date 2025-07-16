using System.Threading;
using System.Threading.Tasks;
using Dapps.CqrsCore.Aggregate;
using Dapps.CqrsCore.Command;
using Dapps.CqrsCore.Event;
using Dapps.CqrsCore.Persistence.Configuration;
using Dapps.CqrsCore.Snapshots;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Dapps.CqrsCore.Persistence.Store
{
    /// <summary>
    /// Default DB context for EventSourcing store which contains Aggregate store, Event Store, Command Store and Snapshot Store
    /// </summary>
    public class EventSourcingDbContext : DbContext, ICommandDbContext, IEventDbContext, ISnapshotDbContext
    {
        private IDbContextTransaction _transaction = null;

        public EventSourcingDbContext(DbContextOptions<EventSourcingDbContext> options) : base(options)
        {

        }

        public DbSet<SerializedAggregate> Aggregates { get; set; }

        public DbSet<SerializedCommand> Commands { get; set; }

        public DbSet<SerializedEvent> Events { get; set; }

        public DbSet<Snapshot> Snapshots { get; set; }

        public void BeginTransaction()
        {
            if (_transaction != null)
            {
                return;
            }

            _transaction = Database.BeginTransaction();
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
            {
                return;
            }

            _transaction = await Database.BeginTransactionAsync();
        }

        public void Commit()
        {
            if (_transaction == null)
            {
                return;
            }

            _transaction.Commit();
            _transaction.Dispose();
            _transaction = null;
        }

        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction == null)
            {
                return;
            }

            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }

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

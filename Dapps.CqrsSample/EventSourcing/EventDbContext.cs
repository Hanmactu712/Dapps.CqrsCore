using System.Threading.Tasks;
using System.Threading;
using Dapps.CqrsCore.Aggregate;
using Dapps.CqrsCore.Event;
using Dapps.CqrsCore.Persistence.Configuration;
using Dapps.CqrsCore.Persistence.Store;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Dapps.CqrsSample.EventSourcing
{
    public class EventDbContext : DbContext, IEventDbContext
    {
        private IDbContextTransaction _transaction;

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

        public void Rollback()
        {
            _transaction.Rollback();
            _transaction.Dispose();
            _transaction = null;
        }

        public async Task RollbackAsync()
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
}

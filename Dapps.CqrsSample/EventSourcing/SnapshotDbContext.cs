using System.Threading.Tasks;
using System.Threading;
using Dapps.CqrsCore.Persistence.Configuration;
using Dapps.CqrsCore.Persistence.Store;
using Dapps.CqrsCore.Snapshots;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Dapps.CqrsSample.EventSourcing
{
    public class SnapshotDbContext : DbContext, ISnapshotDbContext
    {
        private IDbContextTransaction _transaction;

        public SnapshotDbContext(DbContextOptions<SnapshotDbContext> options) : base(options)
        {

        }

        public DbSet<Snapshot> Snapshots { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new SnapshotConfiguration(""));
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

    }
}

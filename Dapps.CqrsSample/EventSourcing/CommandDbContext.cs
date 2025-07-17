using System.Threading.Tasks;
using System.Threading;
using Dapps.CqrsCore.Command;
using Dapps.CqrsCore.Persistence.Configuration;
using Dapps.CqrsCore.Persistence.Store;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Dapps.CqrsSample.EventSourcing
{
    public class CommandDbContext : DbContext, ICommandDbContext
    {
        private IDbContextTransaction _transaction;

        public CommandDbContext(DbContextOptions<CommandDbContext> options) : base(options)
        {

        }

        public DbSet<SerializedCommand> Commands { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new CommandConfiguration(""));
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

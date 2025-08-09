using System.Threading.Tasks;
using System.Threading;
using Dapps.CqrsCore.Snapshots;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Dapps.CqrsCore.Persistence.Store
{
    public interface ISnapshotDbContext
    {
        DbSet<Snapshot> Snapshots { get; set; }

        /// <summary>Saves the changes.</summary>
        /// <returns></returns>
        int SaveChanges();

        /// <summary>
        /// Saves the changes asynchronously.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// provide access to change the tracking information & operations for given entity. 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;

        /// <summary>
        /// Begins a transaction.
        /// </summary>
        void BeginTransaction();

        /// <summary>
        /// Begins a transaction asynchronously.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Commits the transaction.
        /// </summary>
        void Commit();

        /// <summary>
        /// Commits the transaction asynchronously.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task CommitAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// rolelbacks the transaction.
        /// </summary>
        void Rollback();

        /// <summary>
        /// Rollbacks the transaction asynchronously.
        /// </summary>
        /// <returns></returns>
        Task RollbackAsync(CancellationToken cancellation = default);
    }
}

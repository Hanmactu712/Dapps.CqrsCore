using System.Threading;
using System.Threading.Tasks;
using Dapps.CqrsCore.Command;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Dapps.CqrsCore.Persistence.Store
{
    public interface ICommandDbContext
    {
        /// <summary>
        /// DB set for storing command records
        /// </summary>
        DbSet<SerializedCommand> Commands { get; set; }

        /// <summary>Saves the changes.</summary>
        /// <returns></returns>
        int SaveChanges();

        /// <summary>
        /// Asynchronously saves the changes to the database.
        /// </summary>
        /// <returns></returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// provide access to change the tracking information & operations for given entity. 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;

        ///// <summary>
        ///// Asynchronously provide access to change the tracking information & operations for given entity.
        ///// </summary>
        ///// <typeparam name="TEntity"></typeparam>
        ///// <param name="entity"></param>
        ///// <returns></returns>
        //Task<EntityEntry<TEntity>> EntryAsync<TEntity>(TEntity entity) where TEntity : class;

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
    }
}

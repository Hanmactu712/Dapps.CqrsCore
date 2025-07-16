using System.Threading;
using System.Threading.Tasks;
using Dapps.CqrsCore.Aggregate;
using Dapps.CqrsCore.Event;
using Microsoft.EntityFrameworkCore;

namespace Dapps.CqrsCore.Persistence.Store
{
    public interface IEventDbContext
    {
        /// <summary>
        /// Db set of aggregates
        /// </summary>
        DbSet<SerializedAggregate> Aggregates { get; set; }

        /// <summary>
        /// Db set of Events
        /// </summary>
        DbSet<SerializedEvent> Events { get; set; }

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

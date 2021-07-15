using System.Threading.Tasks;
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

        ///// <summary>Saves the changes.</summary>
        ///// <returns></returns>
        //Task<int> SaveChangesAsync();

        EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
    }
}

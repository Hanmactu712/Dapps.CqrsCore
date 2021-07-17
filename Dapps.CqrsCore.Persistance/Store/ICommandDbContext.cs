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
        /// provide access to change the tracking information & operations for given entity. 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
    }
}

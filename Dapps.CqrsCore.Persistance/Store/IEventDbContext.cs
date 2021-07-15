using System.Threading.Tasks;
using Dapps.CqrsCore.Aggregate;
using Dapps.CqrsCore.Event;
using Microsoft.EntityFrameworkCore;

namespace Dapps.CqrsCore.Persistence.Store
{
    public interface IEventDbContext
    {
        DbSet<SerializedAggregate> Aggregates { get; set; }
        DbSet<SerializedEvent> Events { get; set; }

        /// <summary>Saves the changes.</summary>
        /// <returns></returns>
        int SaveChanges();

        ///// <summary>Saves the changes.</summary>
        ///// <returns></returns>
        ////Task<int> SaveChangesAsync();
    }
}

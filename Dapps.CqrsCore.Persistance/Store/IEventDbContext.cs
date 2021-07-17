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
    }
}

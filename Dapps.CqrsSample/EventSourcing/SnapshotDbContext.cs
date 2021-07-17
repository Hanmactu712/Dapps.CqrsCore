using Dapps.CqrsCore.Persistence.Configuration;
using Dapps.CqrsCore.Persistence.Store;
using Dapps.CqrsCore.Snapshots;
using Microsoft.EntityFrameworkCore;

namespace Dapps.CqrsSample.EventSourcing
{
    public class SnapshotDbContext : DbContext, ISnapshotDbContext
    {
        public SnapshotDbContext(DbContextOptions<SnapshotDbContext> options) : base(options)
        {

        }

        public DbSet<Snapshot> Snapshots { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new SnapshotConfiguration(""));
        }
    }
}

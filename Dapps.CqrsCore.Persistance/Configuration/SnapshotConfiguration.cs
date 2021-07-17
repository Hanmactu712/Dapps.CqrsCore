using Dapps.CqrsCore.Snapshots;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dapps.CqrsCore.Persistence.Configuration
{
    /// <summary>
    /// Snapshot configuration for snapshot entity type before saving to database
    /// </summary>
    public class SnapshotConfiguration : IEntityTypeConfiguration<Snapshot>
    {
        private readonly string _schema;

        public SnapshotConfiguration(string schema)
        {
            _schema = schema;
        }

        public void Configure(EntityTypeBuilder<Snapshot> builder)
        {
            builder.ToTable($"{_schema}Snapshots").HasKey(x => x.AggregateId);
            builder.Property(x => x.Version).IsRequired();
            builder.Property(x => x.State).IsRequired().IsUnicode();
            builder.Property(x => x.Time);
        }
    }
}

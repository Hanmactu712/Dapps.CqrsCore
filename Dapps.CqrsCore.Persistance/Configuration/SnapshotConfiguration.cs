using Dapps.CqrsCore.Snapshots;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dapps.CqrsCore.Persistence.Configuration
{
    public class SnapshotConfiguration : IEntityTypeConfiguration<Snapshot>
    {
        private readonly string _schema;

        public SnapshotConfiguration(string schema)
        {
            _schema = schema;
        }

        public void Configure(EntityTypeBuilder<Snapshot> builder)
        {
            builder.ToTable($"{_schema}Snapshots").HasKey(x => x.Id);
            builder.Property(x => x.Version).IsRequired();
            builder.Property(x => x.State).IsRequired().IsUnicode();
        }
    }
}

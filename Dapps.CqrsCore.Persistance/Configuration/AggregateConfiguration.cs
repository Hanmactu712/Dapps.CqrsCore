using Dapps.CqrsCore.Aggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dapps.CqrsCore.Persistence.Configuration
{
    public class AggregateConfiguration : IEntityTypeConfiguration<SerializedAggregate>
    {
        private readonly string _schema;
        public AggregateConfiguration(string schema)
        {
            _schema = schema;
        }

        public void Configure(EntityTypeBuilder<SerializedAggregate> builder)
        {
            builder.ToTable($"{_schema}Aggregates").HasKey(x => x.AggregateId);
            builder.Property(x => x.Class).IsRequired().IsUnicode(false).HasMaxLength(200);
            builder.Property(x => x.Type).IsRequired().IsUnicode(false).HasMaxLength(200);
            builder.Property(x => x.Expires).IsRequired(false);
        }
    }
}

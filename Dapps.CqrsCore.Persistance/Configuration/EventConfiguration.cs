using Dapps.CqrsCore.Event;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dapps.CqrsCore.Persistence.Configuration
{
    public class EventConfiguration : IEntityTypeConfiguration<SerializedEvent>
    {
        private readonly string _schema;

        public EventConfiguration(string schema)
        {
            _schema = schema;
        }

        public void Configure(EntityTypeBuilder<SerializedEvent> builder)
        {
            builder.ToTable($"{_schema}Events")
                .HasKey(x => new { AggregateID = x.AggregateId, x.Version });
            builder.Property(t => t.Class).IsRequired().IsUnicode(false).HasMaxLength(200);
            builder.Property(t => t.Data).IsRequired().IsUnicode();
            builder.Property(t => t.Type).IsRequired().IsUnicode(false).HasMaxLength(200);
        }
    }
}

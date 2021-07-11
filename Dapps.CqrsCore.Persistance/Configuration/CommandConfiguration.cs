using Dapps.CqrsCore.Command;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dapps.CqrsCore.Persistence.Configuration
{
    public class CommandConfiguration : IEntityTypeConfiguration<SerializedCommand>
    {
        private readonly string _schema;

        public CommandConfiguration(string schema)
        {
            _schema = schema;
        }

        public void Configure(EntityTypeBuilder<SerializedCommand> builder)
        {
            builder.ToTable($"{_schema}Commands")
                .HasKey(x => x.CommandId);
            //builder.Property(t => t.Id).IsRequired();
            builder.Property(t => t.Class).IsRequired().IsUnicode(false).HasMaxLength(200);
            builder.Property(t => t.Data).IsRequired().IsUnicode();
            builder.Property(t => t.Type).IsRequired().IsUnicode(false).HasMaxLength(200);
            builder.Property(t => t.UserId).IsRequired();
            builder.Property(t => t.Version).IsRequired(false);
            builder.Property(t => t.SendCancelled).IsRequired(false);
            builder.Property(t => t.SendCompleted).IsRequired(false);
            builder.Property(t => t.SendError).IsRequired(false);
            builder.Property(t => t.SendScheduled).IsRequired(false);
            builder.Property(t => t.SendStarted).IsRequired(false);
            builder.Property(t => t.SendStatus).IsRequired();
        }
    }
}

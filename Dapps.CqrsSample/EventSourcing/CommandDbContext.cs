using Dapps.CqrsCore.Command;
using Dapps.CqrsCore.Persistence.Configuration;
using Dapps.CqrsCore.Persistence.Store;
using Microsoft.EntityFrameworkCore;

namespace Dapps.CqrsSample.EventSourcing
{
    public class CommandDbContext : DbContext, ICommandDbContext
    {
        public CommandDbContext(DbContextOptions<CommandDbContext> options) : base(options)
        {

        }

        public DbSet<SerializedCommand> Commands { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new CommandConfiguration(""));
        }
    }
}

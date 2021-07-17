using System;
using System.Collections.Generic;
using System.Linq;
using Dapps.CqrsCore.Command;
using Dapps.CqrsCore.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Dapps.CqrsCore.Persistence.Store
{
    /// <summary>
    /// Default command store
    /// </summary>
    public class CommandStore : BaseStore<ICommandDbContext>, ICommandStore
    {
        //private readonly DbContextOptions<PersistenceDBContext> ContextOptions;
        //private readonly ICommandDbContext Context;

        public CommandStore(ISerializer serializer, IServiceProvider service):base(service)
        {
            Serializer = serializer ?? throw new ArgumentNullException(nameof(ISerializer));
            //Context = service.CreateScope().ServiceProvider.GetRequiredService<ICommandDbContext>();
            //if(Context == null)
            //    throw new ArgumentNullException(nameof(ICommandDbContext));
        }

        public ISerializer Serializer { get; }

        public bool Exists(Guid commandId)
        {
            return GetDbContext().Commands.Any(c => c.Id.Equals(commandId));
        }

        public SerializedCommand Get(Guid commandId)
        {
            return GetDbContext().Commands.AsNoTracking().FirstOrDefault(c => c.Id.Equals(commandId));
        }

        public IEnumerable<SerializedCommand> GetExpired(DateTimeOffset at)
        {

            return GetDbContext().Commands.AsNoTracking().Where(c => c.SendStatus.Equals(CommandStatus.Scheduled));
        }

        public void Save(SerializedCommand command, bool isNew)
        {
            var dbContext = GetDbContext();
            if (isNew)
            {
                if (command.Id.Equals(Guid.Empty))
                    command.Id = Guid.NewGuid();
                dbContext.Commands.Add(command);
            }
            else
            {
                var entity = dbContext.Entry(command);
                entity.State = EntityState.Modified;
                //Context.Commands
            }

            dbContext.SaveChanges();
        }

        public SerializedCommand Serialize(ICommand command)
        {
            return command.Serialize(Serializer, command.Id, command.Version);
        }
    }
}

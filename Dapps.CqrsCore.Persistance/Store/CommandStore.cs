using System;
using System.Collections.Generic;
using System.Linq;
using Dapps.CqrsCore.Command;
using Dapps.CqrsCore.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Dapps.CqrsCore.Persistence.Store
{
    public class CommandStore : ICommandStore
    {
        //private readonly DbContextOptions<PersistenceDBContext> _dbContextOptions;
        private readonly EventSourcingDBContext _dbContext;
        public CommandStore(ISerializer serializer, EventSourcingDBContext dbContext)
        {
            Serializer = serializer;
            _dbContext = dbContext;
        }

        public ISerializer Serializer { get; }

        public bool Exists(Guid commandID)
        {

            return _dbContext.Commands.Any(c => c.Id.Equals(commandID));
        }

        public SerializedCommand Get(Guid commandID)
        {

            return _dbContext.Commands.AsNoTracking().FirstOrDefault(c => c.Id.Equals(commandID));
        }

        public IEnumerable<SerializedCommand> GetExpired(DateTimeOffset at)
        {

            return _dbContext.Commands.AsNoTracking().Where(c => c.SendStatus.Equals(CommandStatus.Scheduled));
        }

        public void Save(SerializedCommand command, bool isNew)
        {
            if (isNew)
            {
                if (command.Id.Equals(Guid.Empty))
                    command.Id = Guid.NewGuid();
                _dbContext.Commands.Add(command);
            }
            else
            {
                var entity = _dbContext.Entry(command);
                entity.State = EntityState.Modified;
                //_dbContext.Commands
            }

            _dbContext.SaveChanges();
        }

        public SerializedCommand Serialize(ICommand command)
        {
            return command.Serialize(Serializer, command.Id, command.Version);
        }
    }
}

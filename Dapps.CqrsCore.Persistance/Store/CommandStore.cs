using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Dapps.CqrsCore.Command;
using Dapps.CqrsCore.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Dapps.CqrsCore.Persistence.Store
{
    /// <summary>
    /// Default command store
    /// </summary>
    public class CommandStore : BaseStore<ICommandDbContext>, ICommandStore
    {
        private readonly string _offlineStorageFolder;
        private const string DefaultFolder = "Commands";

        public CommandStore(ISerializer serializer, IServiceProvider service, CommandStoreOptions options) : base(service)
        {
            Serializer = serializer ?? throw new ArgumentNullException(nameof(ISerializer));

            _offlineStorageFolder = options?.CommandLocalStorage ??
                                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DefaultFolder);
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

        public IEnumerable<SerializedCommand> GetByAggregateId(Guid aggregateId)
        {
            var dbContext = GetDbContext();
            return dbContext.Commands.AsNoTracking().Where(e => e.AggregateId.Equals(aggregateId)).ToList();
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
            return command.Serialize(Serializer, command.Version);
        }

        public void Box(Guid aggregateId)
        {
            var dbContext = GetDbContext();
            // Serialize the event stream and write it to an external file.
            var commands = GetByAggregateId(aggregateId);
            foreach (var command in commands)
            {
                // Create a new directory using the aggregate identifier as the folder name.
                var path = Path.Combine(_offlineStorageFolder, DefaultFolder, aggregateId.ToString());
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                //get json data from event
                var data = Serializer.Serialize(command);
                var file = Path.Combine(path, $"{command.Id}.json");
                File.WriteAllText(file, data, Encoding.Unicode);

                // Delete the aggregate and the events from the online logs.

                dbContext.Commands.Remove(command);
            }

            dbContext.SaveChanges();
        }

        public IEnumerable<SerializedCommand> UnBox(Guid aggregateId)
        {
            //no need to unbox commands
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapps.CqrsCore.Command;
using Dapps.CqrsCore.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Dapps.CqrsCore.Persistence.Store
{
    /// <summary>
    /// Default command store
    /// </summary>
    public class CommandStore : BaseStore<ICommandDbContext>, ICqrsCommandStore
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

        public async Task<bool> ExistsAsync(Guid commandId)
        {
            return await GetDbContext().Commands.AnyAsync(c => c.Id.Equals(commandId));
        }

        public SerializedCommand Get(Guid commandId)
        {
            return GetDbContext().Commands.AsNoTracking().FirstOrDefault(c => c.Id.Equals(commandId));
        }
        
        public async Task<SerializedCommand> GetAsync(Guid commandId)
        {
            return await GetDbContext().Commands.AsNoTracking().FirstOrDefaultAsync(c => c.Id.Equals(commandId));
        }

        public IEnumerable<SerializedCommand> GetByAggregateId(Guid aggregateId)
        {
            var dbContext = GetDbContext();
            return dbContext.Commands.AsNoTracking().Where(e => e.AggregateId.Equals(aggregateId)).ToList();
        }

        public async Task<IEnumerable<SerializedCommand>> GetByAggregateIdAsync(Guid aggregateId)
        {
            var dbContext = GetDbContext();
            return await dbContext.Commands.AsNoTracking().Where(e => e.AggregateId.Equals(aggregateId)).ToListAsync();
        }

        public IEnumerable<SerializedCommand> GetExpired(DateTimeOffset at)
        {
            return GetDbContext().Commands.AsNoTracking().Where(c => c.SendStatus.Equals(CommandStatus.Scheduled)).ToList();
        }

        public async Task<IEnumerable<SerializedCommand>> GetExpiredAsync(DateTimeOffset at)
        {
            return await GetDbContext().Commands.AsNoTracking().Where(c => c.SendStatus.Equals(CommandStatus.Scheduled)).ToListAsync();
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

        public async Task SaveAsync(SerializedCommand command, bool isNew)
        {
            var dbContext = GetDbContext();
            if (isNew)
            {
                if (command.Id.Equals(Guid.Empty))
                    command.Id = Guid.NewGuid();
                await dbContext.Commands.AddAsync(command);
            }
            else
            {
                var entity = dbContext.Entry(command);
                entity.State = EntityState.Modified;
                //Context.Commands
            }

            await dbContext.SaveChangesAsync();
        }

        public SerializedCommand Serialize(ICqrsCommand command)
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
        
        public async Task BoxAsync(Guid aggregateId)
        {
            var dbContext = GetDbContext();
            // Serialize the event stream and write it to an external file.
            var commands = await GetByAggregateIdAsync(aggregateId);
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

            await dbContext.SaveChangesAsync();
        }

        public IEnumerable<SerializedCommand> UnBox(Guid aggregateId)
        {
            //no need to unbox commands
            throw new NotImplementedException();
        }
                
        public async Task<IEnumerable<SerializedCommand>> UnBoxAsync(Guid aggregateId)
        {
            await Task.CompletedTask;
            //no need to unbox commands
            throw new NotImplementedException();
        }
    }
}

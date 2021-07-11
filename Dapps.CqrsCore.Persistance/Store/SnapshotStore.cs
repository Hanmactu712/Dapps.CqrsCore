using System;
using System.IO;
using System.Linq;
using System.Text;
using Dapps.CqrsCore.Persistence.Exceptions;
using Dapps.CqrsCore.Snapshots;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Dapps.CqrsCore.Persistence.Store
{
    public class SnapshotStore : ISnapshotStore
    {
        private readonly EventSourcingDBContext _dbContext;

        private readonly string _offlineStorageFolder;

        public SnapshotStore(EventSourcingDBContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            var config = configuration.GetSection("Snapshot:LocalStorage").Value;
            if (string.IsNullOrEmpty(config))
                throw new ArgumentNullException(typeof(IConfiguration).FullName);
            _offlineStorageFolder = config;
        }

        public void Box(Guid aggregate)
        {
            // Create a new directory using the aggregate identifier as the folder name.
            var path = Path.Combine(_offlineStorageFolder, aggregate.ToString());
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            // Serialize the event stream and write it to an external file.
            var json = Get(aggregate).State;
            var file = Path.Combine(path, "Snapshot.json");
            File.WriteAllText(file, json, Encoding.Unicode);

            // Delete the aggregate and the events from the online logs.
            Delete(aggregate);
        }

        public Snapshot Get(Guid id)
        {
            return _dbContext.Snapshots.Find(id);
        }

        public void Save(Snapshot snapshot)
        {
            if (!_dbContext.Snapshots.Any(sn => sn.Id.Equals(snapshot.Id)))
                _dbContext.Snapshots.Add(snapshot);
            else
            {
                _dbContext.Entry(snapshot).State = EntityState.Modified;

            }
            _dbContext.SaveChanges();
        }

        public Snapshot Unbox(Guid aggregate)
        {
            // The snapshot must exist!
            var file = Path.Combine(_offlineStorageFolder, aggregate.ToString(), "Snapshot.json");
            if (!File.Exists(file))
                throw new SnapshotNotFoundException(file);

            // Read the serialized JSON into a new snapshot and return it.
            return new Snapshot
            {
                Id = aggregate,
                Version = 1,
                State = File.ReadAllText(file, Encoding.Unicode)
            };
        }

        #region Methods (delete)

        private void Delete(Guid aggregate)
        {
            var snapShot = Get(aggregate);
            if (snapShot != null)
            {
                _dbContext.Snapshots.Remove(snapShot);
                _dbContext.SaveChanges();
            }
        }

        #endregion

    }
}

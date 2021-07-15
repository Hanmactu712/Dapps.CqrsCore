using System;
using Microsoft.EntityFrameworkCore;

namespace Dapps.CqrsCore.Persistence.Options
{
    public class SnapshotStorageOptions 
    {
        public Action<DbContextOptionsBuilder> ConfigureDbContext { get; set; }
    }
}

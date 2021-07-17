using System;
using Dapps.CqrsCore.Snapshots;
using Microsoft.EntityFrameworkCore;

namespace Dapps.CqrsCore.AspNetCore
{
    /// <summary>
    /// Options need to be configured for CQRS service
    /// </summary>
    public class CqrsServiceOptions
    {
        public static string Name = "CqrsServiceConfiguration";
        public bool SaveAll { get; set; } = false;
        public SnapshotOptions Snapshot { get; set; } = SnapshotOptions.Default;
        public Action<DbContextOptionsBuilder> DbContextOption { get; set; }
    }
}

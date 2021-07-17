
using System;
using System.IO;

namespace Dapps.CqrsCore.Snapshots
{
    public class SnapshotOptions
    {
        public int Interval { get; set; } = 200;
        public string LocalStorage { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Snapshots");

        public static SnapshotOptions Default = new SnapshotOptions();
    }
}

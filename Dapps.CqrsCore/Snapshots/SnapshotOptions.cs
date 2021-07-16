
using System;

namespace Dapps.CqrsCore.Snapshots
{
    public class SnapshotOptions
    {
        public int Interval { get; set; } = 200;
        public string LocalStorage { get; set; } = AppDomain.CurrentDomain.BaseDirectory;

        public static SnapshotOptions Default = new SnapshotOptions();
    }
}

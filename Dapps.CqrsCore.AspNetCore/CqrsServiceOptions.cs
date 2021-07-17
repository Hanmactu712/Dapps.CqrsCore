using System;
using System.IO;
using Microsoft.EntityFrameworkCore;

namespace Dapps.CqrsCore.AspNetCore
{
    /// <summary>
    /// Options need to be configured for CQRS service
    /// </summary>
    public class CqrsServiceOptions
    {
        /// <summary>
        /// Default name of the configuration which configured in the appsettings.json
        /// </summary>
        public static string Name = "CqrsServiceConfiguration";
        /// <summary>
        /// Option to save all the commands. If false, only save scheduled commands
        /// </summary>
        public bool SaveAll { get; set; } = false;
        /// <summary>
        /// Local storage path to store command data when boxing & unboxing. Default is {the current location of execution file}/Commands.
        /// </summary>
        public string CommandLocalStorage { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Commands");
        /// <summary>
        /// Local storage path to store event data when boxing & unboxing. Default is {the current location of execution file}/EventSourcing.
        /// </summary>
        public string EventLocalStorage { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EventSourcing");

        /// <summary>
        /// Indicate the snapshot will be taken after how many version of aggregate. Default is 200.
        /// </summary>
        public int Interval { get; set; } = 200;

        /// <summary>
        /// Local storage path to store snapshot data when boxing & unboxing. Default is {the current location of execution file}/Snapshots.
        /// </summary>
        public string SnapshotLocalStorage { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Snapshots");

        /// <summary>
        /// Default Db Builder Option for CQRS service
        /// </summary>
        public Action<DbContextOptionsBuilder> DbContextOption { get; set; }
    }
}

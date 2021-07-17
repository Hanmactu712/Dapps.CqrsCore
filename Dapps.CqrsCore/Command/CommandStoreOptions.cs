using System;
using System.IO;

namespace Dapps.CqrsCore.Command
{
    /// <summary>
    /// Options for command store
    /// </summary>
    public class CommandStoreOptions
    {
        /// <summary>
        /// If SaveAll is true, all the command has been persisted. otherwise, only scheduled command will be persisted in the database.
        /// </summary>
        public bool SaveAll { get; set; } = false;

        /// <summary>
        /// Location to save data of commands when boxing & unboxing
        /// </summary>
        public string CommandLocalStorage { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Commands");
    }
}

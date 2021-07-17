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
        public bool SaveAll { get; set; }
    }
}

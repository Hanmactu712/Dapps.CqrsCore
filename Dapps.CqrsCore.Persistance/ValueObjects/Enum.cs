namespace Dapps.CqrsCore.Persistence.ValueObjects
{
    public enum CommandStatus
    {
        None = 1,
        Scheduled = None << 1,
    }
}

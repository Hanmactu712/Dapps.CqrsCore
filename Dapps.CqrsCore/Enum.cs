
namespace Dapps.CqrsCore
{
    public enum CommandStatus
    {
        None = 0,
        Scheduled = None << 1,
        Started = None << 2,
        Completed = None << 3,
        Cancelled = None << 4,
    }

    public enum MessageCode
    {
        Success = 0,
        InvalidData = 1,
        Exception = 2,
    }
}

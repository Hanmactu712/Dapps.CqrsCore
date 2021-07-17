namespace Dapps.CqrsCore.Command
{
    /// <summary>
    /// Interface of a command handler
    /// </summary>
    /// <typeparam name="TCommand"></typeparam>
    public interface ICommandHandler<in TCommand> where TCommand : ICommand
    {
        /// <summary>
        /// Handle method to handle all command business logic
        /// </summary>
        /// <param name="command"></param>
        void Handle(TCommand command);
    }
}

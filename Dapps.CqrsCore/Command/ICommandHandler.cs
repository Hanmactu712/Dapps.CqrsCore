using System.Threading.Tasks;

namespace Dapps.CqrsCore.Command
{
    public interface ICommandHandler<in TCommand> where TCommand : ICommand
    {
        void Handle(TCommand command);
        Task HandleAsync(TCommand command);
    }
}

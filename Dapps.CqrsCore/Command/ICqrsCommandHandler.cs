using MediatR;

namespace Dapps.CqrsCore.Command;

public interface ICqrsCommandHandler<in TCommand> : IRequestHandler<TCommand>
    where TCommand : ICqrsCommand
{
}

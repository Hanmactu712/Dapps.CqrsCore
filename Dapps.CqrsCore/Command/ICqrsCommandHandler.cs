using MediatR;

namespace Dapps.CqrsCore.Command;

/// <summary>
/// Interface of a command handler
/// </summary>
/// <typeparam name="TCommand"></typeparam>
public interface ICqrsCommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
    where TCommand : ICqrsCommand<TResponse>
{
}

public interface ICqrsCommandHandler<in TCommand> : IRequestHandler<TCommand>
    where TCommand : ICqrsCommand
{
}

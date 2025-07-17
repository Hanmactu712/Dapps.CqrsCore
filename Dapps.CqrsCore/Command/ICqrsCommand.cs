using System;
using MediatR;

namespace Dapps.CqrsCore.Command;

/// <summary>
/// Interface of the command
/// </summary>
public interface ICqrsCommand<TResponse> : IRequest<TResponse>
{
}

public interface ICqrsCommand : IRequest
{
    /// <summary>
    /// unique ID for command
    /// </summary>
    Guid Id { get; set; }

    /// <summary>
    /// Aggregate AggregateId which command is carried for
    /// </summary>
    Guid AggregateId { get; set; }

    /// <summary>
    /// Aggregate version
    /// </summary>
    int? Version { get; set; }
}

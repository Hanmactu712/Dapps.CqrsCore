using System;

namespace Dapps.CqrsCore.Command;

/// <summary>
/// Base class for command
/// </summary>
public abstract class CqrsCommand : ICqrsCommand
{
    /// <summary>
    /// Command unique Id
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// Aggregate unique Id
    /// </summary>
    public Guid AggregateId { get; set; }
    /// <summary>
    /// Command version
    /// </summary>
    public int? Version { get; set; }
    /// <summary>
    /// Time the command is raised
    /// </summary>
    public DateTimeOffset Time { get; set; }

    protected CqrsCommand()
    {
        Id = Guid.NewGuid();
        Time = DateTimeOffset.UtcNow;
        Version = 1;
    }
}

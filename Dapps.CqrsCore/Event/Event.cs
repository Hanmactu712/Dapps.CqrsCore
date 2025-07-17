using System;

namespace Dapps.CqrsCore.Event;

/// <summary>
/// Base class for event message
/// </summary>
public abstract class Event : ICqrsEvent
{
    /// <summary>
    /// Aggregate Id. Version and this fields will be compose unique key for event
    /// </summary>
    public Guid AggregateId { get; set; }
    /// <summary>
    /// event version
    /// </summary>
    public int Version { get; set; }
    /// <summary>
    /// Type of the event
    /// </summary>
    public string EventType { get; set; }
    /// <summary>
    /// Class name of the event message
    /// </summary>
    public string Class { get; set; }
    /// <summary>
    /// Data which event carries
    /// </summary>
    public string Data { get; set; }
    /// <summary>
    /// Time when the event is raised
    /// </summary>
    public DateTimeOffset Time { get; set; }
    /// <summary>
    /// Reference id to track the source where which event is fired. Ex: Command Id
    /// </summary>
    public Guid ReferenceId { get; set; }

    protected Event()
    {
        Time = DateTimeOffset.UtcNow;
    }
}

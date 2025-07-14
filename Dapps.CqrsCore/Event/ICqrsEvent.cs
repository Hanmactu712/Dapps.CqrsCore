using System;
using MediatR;

namespace Dapps.CqrsCore.Event;

/// <summary>
/// Interface for event message
/// </summary>
public interface ICqrsEvent : INotification
{
    /// <summary>
    /// Unique Id of tied aggregate
    /// </summary>
    Guid AggregateId { get; set; }
    /// <summary>
    /// version of aggregate
    /// </summary>
    int Version { get; set; }
    //Guid UserId { get; set; }
    /// <summary>
    /// Timestamp of the event
    /// </summary>
    DateTimeOffset Time { get; set; }
    /// <summary>
    /// Reference to the source which event is fired. It may command Id or other source
    /// </summary>
    Guid ReferenceId { get; set; }
}

﻿using System.Threading;
using System.Threading.Tasks;

namespace Dapps.CqrsCore.Event;

/// <summary>
/// Interface of Event Queue
/// </summary>
public interface ICqrsEventDispatcher
{
    /// <summary>
    /// publish event to queue
    /// </summary>
    /// <param name="ev"></param>
    void Publish(ICqrsEvent ev);

    /// <summary>
    /// Asynchronously subscribe event handler to queue
    /// </summary>
    /// <param name="ev"></param>
    /// <returns></returns>
    Task PublishAsync(ICqrsEvent ev, CancellationToken cancellation = default);
}

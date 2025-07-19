using System;
using System.Threading;
using System.Threading.Tasks;

namespace Dapps.CqrsCore.Snapshots;

/// <summary>
/// Defines the methods needed from the snapshot store.
/// </summary>
public interface ISnapshotStore
{
    /// <summary>
    /// Gets a snapshot from the store.
    /// </summary>
    Snapshot Get(Guid id);

    /// <summary>
    /// Gets a snapshot from the store asynchronously.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<Snapshot> GetAsync(Guid id, CancellationToken cancellation = default);

    /// <summary>
    /// Saves a snapshot to the store.
    /// </summary>
    void Save(Snapshot snapshot);

    /// <summary>
    /// Saves a snapshot to the store asynchronously.
    /// </summary>
    /// <param name="snapshot"></param>
    /// <returns></returns>
    Task SaveAsync(Snapshot snapshot, CancellationToken cancellation = default);

    /// <summary>
    /// Copies a snapshot to offline storage and removes it from online logs.
    /// </summary>
    void Box(Guid id);

    /// <summary>
    /// Copies a snapshot to offline storage and removes it from online logs asynchronously.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task BoxAsync(Guid id, CancellationToken cancellation = default);

    /// <summary>
    /// Retrieves an aggregate from offline storage and returns only its most recent state.
    /// </summary>
    Snapshot Unbox(Guid id);

    /// <summary>
    /// Retrieves an aggregate from offline storage and returns only its most recent state asynchronously.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<Snapshot> UnboxAsync(Guid id, CancellationToken cancellation = default);
}

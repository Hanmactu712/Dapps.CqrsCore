using Dapps.CqrsCore.Event;

namespace Dapps.CqrsCore.Snapshots;

/// <summary>
/// Defines the methods needed from the snapshot repository. The ISnapshotRepository extends the ICqrsEventRepository to include snapshot functionality.
/// </summary>

public interface ISnapshotRepository : ICqrsEventRepository { }
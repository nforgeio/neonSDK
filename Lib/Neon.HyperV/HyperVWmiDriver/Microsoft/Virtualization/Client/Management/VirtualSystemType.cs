namespace Microsoft.Virtualization.Client.Management;

internal enum VirtualSystemType
{
    RealizedSnapshot,
    RecoverySnapshot,
    PlannedSnapshot,
    RealizedVirtualMachine,
    PlannedVirtualMachine,
    MissingSnapshot,
    ReplicaSnapshot,
    ApplicationConsistentReplicaSnapshot,
    ReplicaSnapshotWithSyncedData,
    ReplicaSettings
}

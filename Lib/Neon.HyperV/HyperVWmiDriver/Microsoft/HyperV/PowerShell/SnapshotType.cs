namespace Microsoft.HyperV.PowerShell;

internal enum SnapshotType
{
	Standard = 0,
	Recovery = 1,
	Planned = 2,
	Missing = 5,
	Replica = 6,
	AppConsistentReplica = 7,
	SyncedReplica = 8
}

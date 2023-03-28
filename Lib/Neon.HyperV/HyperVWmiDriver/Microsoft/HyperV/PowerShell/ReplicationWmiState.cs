using System.Diagnostics.CodeAnalysis;

namespace Microsoft.HyperV.PowerShell;

internal enum ReplicationWmiState
{
	Disabled,
	Ready,
	WaitingToCompleteInitialReplication,
	Replicating,
	SyncedReplicationComplete,
	Recovered,
	Committed,
	Suspended,
	Critical,
	WaitingForStartResynchronize,
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Resynchronizing", Justification = "This is the correct API name.")]
	Resynchronizing,
	ResynchronizeSuspended,
	RecoveryInProgress,
	FailbackInProgress,
	FailbackComplete,
	WaitingForUpdateCompletion,
	UpdateCritical,
	Unknown,
	WaitingForRepurposeCompletion,
	PreparedForSyncReplication,
	PreparedForGroupReverseReplication,
	FiredrillInProgress
}

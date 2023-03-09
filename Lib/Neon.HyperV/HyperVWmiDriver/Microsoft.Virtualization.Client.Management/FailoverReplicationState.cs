using System.ComponentModel;

namespace Microsoft.Virtualization.Client.Management;

[TypeConverter(typeof(FailoverReplicationStateConverter))]
internal enum FailoverReplicationState
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

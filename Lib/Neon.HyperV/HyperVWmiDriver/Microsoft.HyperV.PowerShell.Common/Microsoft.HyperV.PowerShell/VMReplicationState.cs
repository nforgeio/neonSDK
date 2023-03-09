using System.Diagnostics.CodeAnalysis;

namespace Microsoft.HyperV.PowerShell;

internal enum VMReplicationState
{
	Disabled,
	ReadyForInitialReplication,
	InitialReplicationInProgress,
	WaitingForInitialReplication,
	Replicating,
	PreparedForFailover,
	FailedOverWaitingCompletion,
	FailedOver,
	Suspended,
	Error,
	WaitingForStartResynchronize,
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Resynchronizing", Justification = "This is the correct API name.")]
	Resynchronizing,
	ResynchronizeSuspended,
	RecoveryInProgress,
	FailbackInProgress,
	FailbackComplete,
	WaitingForUpdateCompletion,
	UpdateError,
	WaitingForRepurposeCompletion,
	PreparedForSyncReplication,
	PreparedForGroupReverseReplication,
	FiredrillInProgress
}

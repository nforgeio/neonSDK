namespace Microsoft.HyperV.PowerShell;

internal enum VMReplicationServerOperationalStatus
{
	Unknown,
	Other,
	Ok,
	Degraded,
	Stressed,
	PredictiveFailure,
	Error,
	NonRecoverableError,
	Starting,
	Stopping,
	Stopped,
	InService,
	NoContact,
	LostCommunication,
	Aborted,
	Dormant,
	SupportingEntityInError,
	Completed,
	PowerMode
}

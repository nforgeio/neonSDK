namespace Microsoft.Virtualization.Client.Management;

internal enum ServiceStatus
{
	Ok,
	Error,
	Degraded,
	Unknown,
	PredictingFailure,
	Starting,
	Stopping,
	Service
}

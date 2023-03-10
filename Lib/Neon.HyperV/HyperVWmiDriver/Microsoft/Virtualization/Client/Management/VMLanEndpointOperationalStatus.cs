namespace Microsoft.Virtualization.Client.Management;

internal enum VMLanEndpointOperationalStatus
{
	Unknown = 0,
	Other = 1,
	Ok = 2,
	Degraded = 3,
	Error = 6,
	NoContact = 12,
	LostCommunication = 13,
	ProtocolMismatch = 32775,
	IovConfigurationError = 32785,
	IovResourceError = 32786,
	IovGuestError = 32787
}

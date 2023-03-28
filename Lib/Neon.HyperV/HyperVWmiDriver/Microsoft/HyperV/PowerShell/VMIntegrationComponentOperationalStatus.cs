using System.Diagnostics.CodeAnalysis;

namespace Microsoft.HyperV.PowerShell;

[SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue", Justification = "This is a mapping of a server defined enum which does not define a zero value.")]
internal enum VMIntegrationComponentOperationalStatus
{
	Ok = 2,
	Degraded = 3,
	Error = 6,
	NonRecoverableError = 7,
	NoContact = 12,
	LostCommunication = 13,
	Disabled = 32896,
	ProtocolMismatch = 32775,
	ApplicationCritical = 32782,
	CommunicationTimedOut = 32783,
	CommunicationFailed = 32784
}

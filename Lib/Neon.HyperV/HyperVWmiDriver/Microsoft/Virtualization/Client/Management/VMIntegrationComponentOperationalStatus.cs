using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Virtualization.Client.Management;

[TypeConverter(typeof(VMIntegrationComponentOperationalStatusConverter))]
[SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue", Justification = "This is a mapping of a server defined enum which does not define a zero value.")]
internal enum VMIntegrationComponentOperationalStatus
{
	Unknown = 0,
	Ok = 2,
	Degraded = 3,
	Error = 6,
	NonRecoverableError = 7,
	NoContact = 12,
	LostCommunication = 13,
	Dormant = 15,
	Disabled = 32896,
	ApplicationCritical = 32782,
	Mismatch = 32775,
	CommunicationTimedOut = 32783,
	CommunicationFailed = 32784
}

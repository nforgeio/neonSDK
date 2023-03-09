using System.ComponentModel;

namespace Microsoft.Virtualization.Client.Management;

[TypeConverter(typeof(VMHeartbeatStatusConverter))]
internal enum VMHeartbeatStatus
{
	Unknown,
	Disabled,
	NoContact,
	Error,
	LostCommunication,
	OkApplicationsUnknown,
	OkApplicationsHealthy,
	OkApplicationsCritical,
	Paused
}

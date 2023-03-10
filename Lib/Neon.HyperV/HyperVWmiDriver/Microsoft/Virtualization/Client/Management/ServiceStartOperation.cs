using System.ComponentModel;

namespace Microsoft.Virtualization.Client.Management;

[TypeConverter(typeof(EnumResourceConverter<ServiceStartOperation>))]
internal enum ServiceStartOperation
{
	None = 2,
	RestartIfPreviouslyRunning,
	AlwaysStartup
}

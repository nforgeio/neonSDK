using System.ComponentModel;

namespace Microsoft.Virtualization.Client.Management;

[TypeConverter(typeof(EnhancedSessionTransportTypeConverter))]
internal enum EnhancedSessionTransportType
{
	VMBus,
	HvSocket
}

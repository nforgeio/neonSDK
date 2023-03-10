using System.ComponentModel;

namespace Microsoft.Virtualization.Client.Management;

[TypeConverter(typeof(EnumResourceConverter<VMMigrationTransportType>))]
internal enum VMMigrationTransportType
{
	TCP = 5,
	SMB = 32768
}

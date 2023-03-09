using System.ComponentModel;

namespace Microsoft.Virtualization.Client.Management;

[TypeConverter(typeof(EnumResourceConverter<VirtualHardDiskType>))]
internal enum VirtualHardDiskType
{
	Unknown = 0,
	FixedSize = 2,
	DynamicallyExpanding = 3,
	Differencing = 4
}

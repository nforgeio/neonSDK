using System.ComponentModel;

namespace Microsoft.Virtualization.Client.Management;

[TypeConverter(typeof(EnumResourceConverter<VirtualHardDiskPmemAddressAbstractionType>))]
internal enum VirtualHardDiskPmemAddressAbstractionType
{
    None = 0,
    BTT = 1,
    Unknown = 65535
}

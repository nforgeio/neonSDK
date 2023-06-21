using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Virtualization.Client.Management;

[TypeConverter(typeof(EnumResourceConverter<VirtualHardDiskFormat>))]
internal enum VirtualHardDiskFormat
{
    Unknown = 0,
    Vhd = 2,
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    Vhdx = 3,
    VHDSet = 4
}

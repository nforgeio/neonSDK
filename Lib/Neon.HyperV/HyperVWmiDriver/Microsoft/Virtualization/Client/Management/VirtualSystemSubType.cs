using System.ComponentModel;

namespace Microsoft.Virtualization.Client.Management;

[TypeConverter(typeof(VirtualSystemSubTypeConverter))]
internal enum VirtualSystemSubType
{
    Unknown,
    Type1,
    Type2
}

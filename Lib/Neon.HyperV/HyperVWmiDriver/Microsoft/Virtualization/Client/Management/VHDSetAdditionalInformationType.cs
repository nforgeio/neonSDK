using System.ComponentModel;

namespace Microsoft.Virtualization.Client.Management;

[TypeConverter(typeof(EnumResourceConverter<VHDSetAdditionalInformationType>))]
internal enum VHDSetAdditionalInformationType
{
    Unknown,
    Other,
    Paths
}

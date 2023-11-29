using System.ComponentModel;

namespace Microsoft.Virtualization.Client.Management;

[TypeConverter(typeof(EnumResourceConverter<VHDSnapshotAdditionalInformationType>))]
internal enum VHDSnapshotAdditionalInformationType
{
    Unknown,
    Other,
    ParentPaths
}

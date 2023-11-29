using System.ComponentModel;

namespace Microsoft.Virtualization.Client.Management;

[TypeConverter(typeof(EnumResourceConverter<VMMigrationType>))]
internal enum VMMigrationType
{
    Unknown = 0,
    VirtualSystem = 32768,
    Storage = 32769,
    PlannedVirtualSystem = 32770,
    VirtualSystemAndStorage = 32771
}

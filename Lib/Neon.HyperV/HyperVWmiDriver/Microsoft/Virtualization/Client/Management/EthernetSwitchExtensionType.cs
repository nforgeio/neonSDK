using System.ComponentModel;

namespace Microsoft.Virtualization.Client.Management;

[TypeConverter(typeof(EnumResourceConverter<EthernetSwitchExtensionType>))]
internal enum EthernetSwitchExtensionType
{
    Unknown,
    Capture,
    Filter,
    Forward,
    Monitoring,
    Native
}

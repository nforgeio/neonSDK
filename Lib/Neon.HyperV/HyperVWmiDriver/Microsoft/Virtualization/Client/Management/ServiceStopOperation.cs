using System.ComponentModel;

namespace Microsoft.Virtualization.Client.Management;

[TypeConverter(typeof(EnumResourceConverter<ServiceStopOperation>))]
internal enum ServiceStopOperation
{
    PowerOff = 2,
    SaveState,
    Shutdown
}

using System.ComponentModel;

namespace Microsoft.Virtualization.Client.Management;

[TypeConverter(typeof(EnhancedSessionModeStateTypeConverter))]
internal enum EnhancedSessionModeStateType
{
    Available = 2,
    Disabled = 3,
    Enabled = 6
}

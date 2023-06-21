using System.ComponentModel;

namespace Microsoft.HyperV.PowerShell;

[TypeConverter(typeof(ICStatusEnumResourceConverter))]
internal enum ICStatus
{
    None,
    UpToDate,
    RequiresUpdate
}

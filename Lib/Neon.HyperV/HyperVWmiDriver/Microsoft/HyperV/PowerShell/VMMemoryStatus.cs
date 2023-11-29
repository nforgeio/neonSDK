using System.ComponentModel;

namespace Microsoft.HyperV.PowerShell;

[TypeConverter(typeof(VMMemoryStatusEnumResourceConverter))]
internal enum VMMemoryStatus
{
    None,
    Paging,
    Ok,
    Low,
    Warning,
    Spanning
}

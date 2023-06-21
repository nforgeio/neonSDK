using System.ComponentModel;

namespace Microsoft.HyperV.PowerShell;

[TypeConverter(typeof(ThreadCountEnumResourceConverter))]
internal enum ThreadCount
{
    Automatic,
    Low,
    Medium,
    High
}

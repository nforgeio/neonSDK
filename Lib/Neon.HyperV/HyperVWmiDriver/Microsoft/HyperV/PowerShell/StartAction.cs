using System.Diagnostics.CodeAnalysis;

namespace Microsoft.HyperV.PowerShell;

[SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]
internal enum StartAction
{
    Nothing = 2,
    StartIfRunning,
    Start
}

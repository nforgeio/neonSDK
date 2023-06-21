using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Virtualization.Client.Management;

[TypeConverter(typeof(VMComputerSystemStateConverter))]
[SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue", Justification = "This is a mapping of a server defined enum which does not define a zero value.")]
internal enum VMComputerSystemState
{
    Unknown = 0,
    Other = 1,
    Running = 2,
    PowerOff = 3,
    Stopping = 4,
    Saved = 6,
    Paused = 9,
    Starting = 10,
    Reset = 11,
    Saving = 32773,
    Pausing = 32776,
    Resuming = 32777,
    FastSaved = 32779,
    FastSaving = 32780,
    ForceShutdown = 32781,
    ForceReboot = 32782,
    Hibernated = 32783,
    ComponentServicing = 32784
}

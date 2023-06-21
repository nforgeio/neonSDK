using System.Diagnostics.CodeAnalysis;

namespace Microsoft.HyperV.PowerShell;

[SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue", Justification = "Doesn't make sense to have a zero here as it's defined by DMTF standard.")]
internal enum VMState
{
    Other = 1,
    Running = 2,
    Off = 3,
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
    ComponentServicing = 32784,
    RunningCritical = 32785,
    OffCritical = 32786,
    StoppingCritical = 32787,
    SavedCritical = 32788,
    PausedCritical = 32789,
    StartingCritical = 32790,
    ResetCritical = 32791,
    SavingCritical = 32792,
    PausingCritical = 32793,
    ResumingCritical = 32794,
    FastSavedCritical = 32795,
    FastSavingCritical = 32796
}

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Virtualization.Client.Management;

[SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue", Justification = "This is a mapping of a server defined enum which does not define a zero value.")]
internal enum VMComputerSystemOperationalStatus
{
    Unknown = 0,
    Ok = 2,
    Degraded = 3,
    PredictiveFailure = 5,
    InService = 11,
    SupportingEntityInError = 16,
    CreatingSnapshot = 32768,
    ApplyingSnapshot = 32769,
    DeletingSnapshot = 32770,
    WaitingToStart = 32771,
    MergingDisks = 32772,
    ExportingVirtualMachine = 32773,
    MigratingVirtualMachine = 32774,
    BackingUpVirtualMachine = 32776,
    ModifyingUpVirtualMachine = 32777,
    StorageMigrationPhaseOne = 32778,
    StorageMigrationPhaseTwo = 32779,
    MigratingPlannedVm = 32780,
    CheckingCompatibility = 32781,
    ApplicationCriticalState = 32782,
    CommunicationTimedOut = 32783,
    CommunicationFailed = 32784,
    NoIommu = 32785,
    NoIovSupportInNic = 32786,
    SwitchNotInIovMode = 32787,
    IovBlockedByPolicy = 32788,
    IovNoAvailResources = 32789,
    IovGuestDriversNeeded = 32790,
    CriticalIoError = 32795
}

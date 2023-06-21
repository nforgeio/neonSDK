namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_AffectedJobElement")]
internal interface IVMTaskAssociationObject
{
    bool IsActingOnVmComputerSystem { get; }

    string VmComputerSystemInstanceId { get; }

    IVMTask GetTask();
}

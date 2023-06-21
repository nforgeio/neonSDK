namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_ShutdownComponent")]
internal interface IVMShutdownComponent : IVMIntegrationComponent, IVMDevice, IVirtualizationManagementObject
{
    long MachineLockedErrorCode { get; }

    void InitiateReboot(bool force, string reason);

    void InitiateShutdown(bool force, string reason);
}

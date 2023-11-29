namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_SecurityElement")]
internal interface IVMSecurityInformation : IVirtualizationManagementObject
{
    bool Shielded { get; }

    bool EncryptStateAndVmMigrationTrafficEnabled { get; }
}

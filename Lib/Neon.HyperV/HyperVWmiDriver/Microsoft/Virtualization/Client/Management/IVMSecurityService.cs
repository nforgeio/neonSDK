namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_SecurityService")]
internal interface IVMSecurityService : IVirtualizationManagementObject
{
    IVMTask BeginSetKeyProtector(IVMSecuritySetting securitySettingData, byte[] rawKeyProtector);

    void EndSetKeyProtector(IVMTask task);

    byte[] GetKeyProtector(IVMSecuritySetting securitySettingData);

    IVMTask BeginRestoreLastKnownGoodKeyProtector(IVMSecuritySetting securitySettingData);

    void EndRestoreLastKnownGoodKeyProtector(IVMTask task);
}

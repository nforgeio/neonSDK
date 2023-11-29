namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_SecuritySettingData")]
internal interface IVMSecuritySetting : IVirtualizationManagementObject, IPutableAsync, IPutable
{
    bool TpmEnabled { get; set; }

    bool KsdEnabled { get; set; }

    bool ShieldingRequested { get; set; }

    bool EncryptStateAndVmMigrationTraffic { get; set; }

    bool DataProtectionRequested { get; set; }

    bool VirtualizationBasedSecurityOptOut { get; set; }

    bool BindToHostTpm { get; set; }
}

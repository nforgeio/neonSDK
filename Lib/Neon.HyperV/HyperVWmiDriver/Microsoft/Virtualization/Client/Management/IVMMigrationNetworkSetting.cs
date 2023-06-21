namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_VirtualSystemMigrationNetworkSettingData")]
internal interface IVMMigrationNetworkSetting : IVirtualizationManagementObject, IPutableAsync, IPutable
{
    string SubnetNumber { get; set; }

    byte PrefixLength { get; set; }

    uint Metric { get; set; }

    string[] Tags { get; set; }
}

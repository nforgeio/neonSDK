namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_VirtualEthernetSwitchNicTeamingSettingData")]
internal interface IEthernetSwitchNicTeamingFeature : IEthernetSwitchFeature, IEthernetFeature, IVirtualizationManagementObject
{
    uint TeamingMode { get; set; }

    uint LoadBalancingAlgorithm { get; set; }
}

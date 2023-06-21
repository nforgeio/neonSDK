namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_EthernetSwitchHardwareOffloadSettingData")]
internal interface IEthernetSwitchOffloadFeature : IEthernetSwitchFeature, IEthernetFeature, IVirtualizationManagementObject
{
    bool DefaultQueueVrssEnabled { get; set; }

    bool DefaultQueueVmmqEnabled { get; set; }

    uint DefaultQueueVmmqQueuePairs { get; set; }

    uint DefaultQueueVrssMinQueuePairs { get; set; }

    uint DefaultQueueVrssQueueSchedulingMode { get; set; }

    bool DefaultQueueVrssExcludePrimaryProcessor { get; set; }

    bool DefaultQueueVrssIndependentHostSpreading { get; set; }

    bool SoftwareRscEnabled { get; set; }
}

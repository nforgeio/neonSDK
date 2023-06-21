namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_EthernetSwitchPortOffloadSettingData")]
internal interface IEthernetSwitchPortOffloadFeature : IEthernetSwitchPortFeature, IEthernetFeature, IVirtualizationManagementObject
{
    int VMQOffloadWeight { get; set; }

    int IOVOffloadWeight { get; set; }

    int IPSecOffloadLimit { get; set; }

    uint IOVQueuePairsRequested { get; set; }

    IovInterruptModerationMode InterruptModerationMode { get; set; }

    uint PacketDirectNumProcs { get; set; }

    uint PacketDirectModerationCount { get; set; }

    uint PacketDirectModerationInterval { get; set; }

    bool VrssEnabled { get; set; }

    bool VmmqEnabled { get; set; }

    uint VmmqQueuePairs { get; set; }

    uint VrssMinQueuePairs { get; set; }

    uint VrssQueueSchedulingMode { get; set; }

    bool VrssExcludePrimaryProcessor { get; set; }

    bool VrssIndependentHostSpreading { get; set; }

    uint VrssVmbusChannelAffinityPolicy { get; set; }
}

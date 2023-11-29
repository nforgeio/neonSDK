namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_EthernetSwitchHardwareOffloadData")]
internal interface IEthernetSwitchOffloadStatus : IEthernetSwitchStatus, IEthernetStatus, IVirtualizationManagementObject
{
    uint IovQueuePairCapacity { get; }

    uint IovQueuePairUsage { get; }

    uint IovVfCapacity { get; }

    uint IovVfUsage { get; }

    uint IPsecSACapacity { get; }

    uint IPsecSAUsage { get; }

    uint VmqCapacity { get; }

    uint VmqUsage { get; }

    bool PacketDirectInUse { get; }

    bool DefaultQueueVrssEnabled { get; }

    bool DefaultQueueVmmqEnabled { get; }

    uint DefaultQueueVmmqQueuePairs { get; }

    uint DefaultQueueVrssMinQueuePairs { get; }

    uint DefaultQueueVrssQueueSchedulingMode { get; }

    bool DefaultQueueVrssExcludePrimaryProcessor { get; }

    bool DefaultQueueVrssIndependentHostSpreading { get; }
}

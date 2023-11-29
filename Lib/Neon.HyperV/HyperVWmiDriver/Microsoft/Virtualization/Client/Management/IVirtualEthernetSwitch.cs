using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_VirtualEthernetSwitch")]
internal interface IVirtualEthernetSwitch : IVirtualSwitch, IVirtualizationManagementObject, IPutable
{
    IVirtualEthernetSwitchSetting Setting { get; }

    IEnumerable<IVirtualEthernetSwitchPort> SwitchPorts { get; }

    bool IsDefaultSwitch { get; }

    IEthernetSwitchOffloadStatus OffloadStatus { get; }

    IEthernetSwitchBandwidthStatus BandwidthStatus { get; }

    IEnumerable<IEthernetSwitchStatus> GetRuntimeStatuses();
}

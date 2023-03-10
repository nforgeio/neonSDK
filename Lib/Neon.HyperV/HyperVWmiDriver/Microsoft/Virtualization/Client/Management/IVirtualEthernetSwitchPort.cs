using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_EthernetSwitchPort")]
internal interface IVirtualEthernetSwitchPort : IEthernetPort, IVirtualSwitchPort, IVirtualizationManagementObject
{
	IVirtualEthernetSwitch VirtualEthernetSwitch { get; }

	IVirtualEthernetSwitchPortSetting Setting { get; }

	IEthernetSwitchPortOffloadStatus OffloadStatus { get; }

	IEthernetSwitchPortBandwidthStatus BandwidthStatus { get; }

	IEnumerable<IEthernetPortStatus> GetRuntimeStatuses();
}

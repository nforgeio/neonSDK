using System;
using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_VirtualEthernetSwitchManagementService")]
internal interface IVirtualSwitchManagementService : IVirtualizationManagementObject, IEthernetSwitchFeatureService
{
	IEnumerable<IVirtualEthernetSwitch> VirtualSwitches { get; }

	IEnumerable<IExternalNetworkPort> ExternalNetworkPorts { get; }

	IEnumerable<IInternalEthernetPort> InternalEthernetPorts { get; }

	IVirtualEthernetSwitchManagementCapabilities Capabilities { get; }

	IVMTask BeginCreateVirtualSwitch(string friendlyName, string instanceId, string notes, bool iovPreferred, BandwidthReservationMode? bandwidthReservationMode, bool? packetDirectEnabled, bool? embeddedTeamingEnabled);

	IVirtualEthernetSwitch EndCreateVirtualSwitch(IVMTask task);

	IVMTask BeginDeleteVirtualSwitch(IVirtualEthernetSwitch virtualSwitch);

	void EndDeleteVirtualSwitch(IVMTask task);

	IVMTask BeginAddVirtualSwitchPorts(IVirtualEthernetSwitch virtualSwitch, IEthernetPortAllocationSettingData[] portsToAdd);

	IEnumerable<IEthernetPortAllocationSettingData> EndAddVirtualSwitchPorts(IVMTask task);

	IVMTask BeginRemoveVirtualSwitchPorts(IVirtualEthernetSwitchPort[] portsToRemove);

	void EndRemoveVirtualSwitchPorts(IVMTask task);

	IVMTask BeginModifyVirtualSwitchPorts(IVirtualEthernetSwitchPortSetting[] portsToModify);

	void EndModifyVirtualSwitchPorts(IVMTask task);

	void UpdateSwitches(TimeSpan threshold);

	void UpdateExternalNetworkPorts(TimeSpan threshold);

	void UpdateInternalEthernetPorts(TimeSpan threshold);
}

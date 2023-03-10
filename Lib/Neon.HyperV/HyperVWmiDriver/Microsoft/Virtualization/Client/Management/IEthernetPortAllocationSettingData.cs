using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_EthernetPortAllocationSettingData")]
internal interface IEthernetPortAllocationSettingData : IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable
{
	WmiObjectPath[] HostResources { get; set; }

	WmiObjectPath HostResource { get; set; }

	string Address { get; set; }

	string TestReplicaPoolId { get; set; }

	string TestReplicaSwitchName { get; set; }

	IEnumerable<IEthernetSwitchPortFeature> Features { get; }
}

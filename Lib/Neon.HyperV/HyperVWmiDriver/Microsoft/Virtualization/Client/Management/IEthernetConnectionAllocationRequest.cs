using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_EthernetPortAllocationSettingData", PrimaryMapping = false)]
internal interface IEthernetConnectionAllocationRequest : IEthernetPortAllocationSettingData, IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable, IDeleteableAsync, IDeleteable
{
	IEthernetPortSetting Parent { get; set; }

	bool IsEnabled { get; set; }

	IResourcePool ResourcePool { get; }

	IReadOnlyList<string> RequiredFeatureIds { get; set; }

	IReadOnlyList<string> RequiredFeatureNames { get; }

	int TestNetworkConnectivity(bool isSender, string senderIPAddress, string receiverIPAddress, string receiverMacAddress, int isolationID, int sequenceNumber, int payloadSize);
}

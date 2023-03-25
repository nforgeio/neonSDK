using System;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("CIM_EthernetPort")]
internal interface IEthernetPort : IVirtualSwitchPort, IVirtualizationManagementObject
{
	string DeviceId { get; }

	string PermanentAddress { get; }

	ILanEndpoint LanEndpoint { get; }

	IEthernetPort GetConnectedEthernetPort(TimeSpan threshold);
}

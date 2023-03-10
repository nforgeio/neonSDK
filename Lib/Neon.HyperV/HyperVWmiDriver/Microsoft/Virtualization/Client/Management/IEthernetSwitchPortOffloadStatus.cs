namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_EthernetSwitchPortOffloadData")]
internal interface IEthernetSwitchPortOffloadStatus : IEthernetPortStatus, IEthernetStatus, IVirtualizationManagementObject
{
	int VmqOffloadUsage { get; }

	int IovOffloadUsage { get; }

	uint IovQueuePairUsage { get; }

	bool IovOffloadActive { get; }

	uint IpsecCurrentOffloadSaCount { get; }

	uint VmqId { get; }

	ushort IovVirtualFunctionId { get; }

	bool VrssEnabled { get; }

	bool VmmqEnabled { get; }

	uint VmmqQueuePairs { get; }

	uint VrssMinQueuePairs { get; }

	uint VrssQueueSchedulingMode { get; }

	bool VrssExcludePrimaryProcessor { get; }

	bool VrssIndependentHostSpreading { get; }

	uint VrssVmbusChannelAffinityPolicy { get; }
}

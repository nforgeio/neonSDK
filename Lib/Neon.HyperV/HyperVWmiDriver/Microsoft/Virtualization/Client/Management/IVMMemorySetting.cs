namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_MemorySettingData")]
internal interface IVMMemorySetting : IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable, IMetricMeasurableElement
{
	long AllocatedRam { get; set; }

	long MinimumMemory { get; set; }

	long MaximumMemory { get; set; }

	long MaximumMemoryPerNumaNode { get; set; }

	int PriorityLevel { get; set; }

	int TargetMemoryBuffer { get; set; }

	bool IsDynamicMemoryEnabled { get; set; }
}

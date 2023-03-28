namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_GpuPartition")]
internal interface IVMGpuPartitionAdapter : IVMDevice, IVirtualizationManagementObject
{
	string DeviceInstancePath { get; }

	ulong PartitionId { get; }
}

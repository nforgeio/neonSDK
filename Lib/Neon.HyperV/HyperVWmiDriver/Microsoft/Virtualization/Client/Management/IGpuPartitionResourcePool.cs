namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_ResourcePool", PrimaryMapping = false)]
internal interface IGpuPartitionResourcePool : IResourcePool, IVirtualizationManagementObject, IDeleteableAsync, IDeleteable, IMetricMeasurableElement
{
}

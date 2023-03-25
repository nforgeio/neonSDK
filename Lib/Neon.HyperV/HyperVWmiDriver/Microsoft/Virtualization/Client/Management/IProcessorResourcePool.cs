namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_ProcessorPool")]
internal interface IProcessorResourcePool : IResourcePool, IVirtualizationManagementObject, IDeleteableAsync, IDeleteable, IMetricMeasurableElement
{
}

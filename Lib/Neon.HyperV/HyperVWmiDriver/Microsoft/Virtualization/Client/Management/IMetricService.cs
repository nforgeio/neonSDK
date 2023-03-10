namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_MetricService")]
internal interface IMetricService : IVirtualizationManagementObject
{
	IMetricServiceSetting Setting { get; }

	void ControlMetrics(IMetricMeasurableElement targetObject, IMetricDefinition metricDefinition, MetricEnabledState targetState);
}

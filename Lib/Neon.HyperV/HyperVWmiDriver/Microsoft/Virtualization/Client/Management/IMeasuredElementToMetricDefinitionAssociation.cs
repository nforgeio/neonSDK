namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_MetricDefForME")]
internal interface IMeasuredElementToMetricDefinitionAssociation : IVirtualizationManagementObject
{
    MetricEnabledState EnabledState { get; }
}

namespace Microsoft.Virtualization.Client.Management;

internal sealed class MeasuredElementToMetricDefinitionAssociationView : View, IMeasuredElementToMetricDefinitionAssociation, IVirtualizationManagementObject
{
	internal static class WmiMemberNames
	{
		public const string MetricCollectionEnabled = "MetricCollectionEnabled";
	}

	public MetricEnabledState EnabledState => GetProperty<MetricEnabledState>("MetricCollectionEnabled");
}

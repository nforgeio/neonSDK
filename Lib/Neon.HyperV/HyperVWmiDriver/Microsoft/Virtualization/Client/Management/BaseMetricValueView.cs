using System;

namespace Microsoft.Virtualization.Client.Management;

internal sealed class BaseMetricValueView : MetricValueView, IBaseMetricValue, IMetricValue, IVirtualizationManagementObject
{
	public override TimeSpan Duration => GetProperty<TimeSpan>("Duration");
}

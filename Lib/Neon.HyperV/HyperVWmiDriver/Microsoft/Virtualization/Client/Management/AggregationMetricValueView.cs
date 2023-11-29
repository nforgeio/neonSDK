using System;

namespace Microsoft.Virtualization.Client.Management;

internal sealed class AggregationMetricValueView : MetricValueView, IAggregationMetricValue, IMetricValue, IVirtualizationManagementObject
{
    public override TimeSpan Duration => GetProperty<TimeSpan>("AggregationDuration");
}

using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal interface IMetricMeasurableElement : IVirtualizationManagementObject
{
    MetricEnabledState AggregateMetricEnabledState { get; }

    IReadOnlyCollection<IMetricValue> GetMetricValues();
}

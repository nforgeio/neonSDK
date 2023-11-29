using System;
using System.Globalization;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class MetricValueView : View, IMetricValue, IVirtualizationManagementObject
{
    internal static class WmiMemberNames
    {
        public const string AggregationDuration = "AggregationDuration";

        public const string Duration = "Duration";

        public const string MetricDefinitionId = "MetricDefinitionId";

        public const string MetricValue = "MetricValue";

        public const string BreakdownValue = "BreakdownValue";
    }

    public abstract TimeSpan Duration { get; }

    public ulong IntegerValue => ulong.Parse(RawValue, CultureInfo.InvariantCulture);

    public string RawValue => GetProperty<string>("MetricValue");

    public string BreakdownValue => GetProperty<string>("BreakdownValue");

    public MetricType MetricType => MetricDefinitionView.MapMetricIdToMetricType(GetProperty<string>("MetricDefinitionId"));
}

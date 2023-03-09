using System;

namespace Microsoft.Virtualization.Client.Management;

internal interface IMetricValue : IVirtualizationManagementObject
{
	TimeSpan Duration { get; }

	ulong IntegerValue { get; }

	string RawValue { get; }

	string BreakdownValue { get; }

	MetricType MetricType { get; }
}

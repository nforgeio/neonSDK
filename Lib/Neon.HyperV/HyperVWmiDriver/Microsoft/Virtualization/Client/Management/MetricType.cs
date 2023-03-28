namespace Microsoft.Virtualization.Client.Management;

internal enum MetricType
{
	Unknown,
	AverageMemory,
	MaximumMemory,
	MinimumMemory,
	AverageCpu,
	AverageDiskLatency,
	AverageDiskThroughput,
	MaximumDiskAllocation,
	DiskNormalizedIOCount,
	DiskDataWritten,
	DiskDataRead,
	OutgoingNetworkTraffic,
	IncomingNetworkTraffic,
	AggregatedAverageMemory,
	AggregatedMaximumMemory,
	AggregatedMinimumMemory,
	AggregatedAverageCpu,
	AggregatedAverageDiskLatency,
	AggregatedAverageDiskThroughput,
	AggregatedMaximumDiskAllocation,
	AggregatedDiskNormalizedIOCount,
	AggregatedDiskDataWritten,
	AggregatedDiskDataRead,
	AggregatedOutgoingNetworkTraffic,
	AggregatedIncomingNetworkTraffic
}

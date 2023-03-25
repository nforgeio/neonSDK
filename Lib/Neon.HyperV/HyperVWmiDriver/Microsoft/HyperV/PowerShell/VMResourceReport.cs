using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Management.Infrastructure;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal abstract class VMResourceReport
{
	private Server m_Server;

	private TimeSpan? m_ShortestDuration;

	private TimeSpan? m_LongestDuration;

	public CimSession CimSession => m_Server.Session.Session;

	public string ComputerName => m_Server.UserSpecifiedName;

	public TimeSpan? MeteringDuration
	{
		get
		{
			TimeSpan? result = null;
			if (m_LongestDuration.HasValue && m_ShortestDuration.HasValue && m_LongestDuration.Value - m_ShortestDuration.Value <= TimeSpan.FromSeconds(30.0))
			{
				result = TimeSpan.FromMilliseconds((m_LongestDuration.Value + m_ShortestDuration.Value).TotalMilliseconds / 2.0);
			}
			return result;
		}
	}

	public ulong? AverageProcessorUsage { get; private set; }

	public ulong? AverageMemoryUsage { get; private set; }

	public ulong? MaximumMemoryUsage { get; private set; }

	public ulong? MinimumMemoryUsage { get; private set; }

	public ulong? TotalDiskAllocation { get; private set; }

	public ulong? AggregatedAverageNormalizedIOPS { get; private set; }

	public ulong? AggregatedAverageLatency { get; private set; }

	public ulong? AggregatedDiskDataRead { get; private set; }

	public ulong? AggregatedDiskDataWritten { get; private set; }

	public ulong? AggregatedNormalizedIOCount { get; private set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	public VMPortAclMeteringReport[] NetworkMeteredTrafficReport { get; protected set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	public VHDMetrics[] HardDiskMetrics { get; protected set; }

	internal VMResourceReport(Server server)
	{
		if (server == null)
		{
			throw new ArgumentNullException("server");
		}
		m_Server = server;
	}

	protected void UpdateMeteringDuration(IEnumerable<IMetricValue> values)
	{
		IOrderedEnumerable<IMetricValue> source = from value in values
			where value.MetricType != MetricType.AggregatedAverageDiskLatency && value.MetricType != MetricType.AggregatedAverageDiskThroughput
			orderby value.Duration descending
			select value;
		UpdateMeteringDuration(source.First().Duration, source.Last().Duration);
	}

	protected void UpdateMeteringDuration(TimeSpan longestDuration, TimeSpan shortestDuration)
	{
		if (!m_ShortestDuration.HasValue || m_ShortestDuration > shortestDuration)
		{
			m_ShortestDuration = shortestDuration;
		}
		if (!m_LongestDuration.HasValue || m_LongestDuration < longestDuration)
		{
			m_LongestDuration = longestDuration;
		}
	}

	protected void ParseMetricValues(IList<IMetricValue> values)
	{
		foreach (IMetricValue value in values)
		{
			switch (value.MetricType)
			{
			case MetricType.AggregatedAverageMemory:
				AverageMemoryUsage = value.IntegerValue;
				break;
			case MetricType.AggregatedMaximumMemory:
				MaximumMemoryUsage = value.IntegerValue;
				break;
			case MetricType.AggregatedMinimumMemory:
				MinimumMemoryUsage = value.IntegerValue;
				break;
			case MetricType.AggregatedAverageCpu:
				AverageProcessorUsage = value.IntegerValue;
				break;
			case MetricType.AggregatedAverageDiskLatency:
				AggregatedAverageLatency = value.IntegerValue;
				break;
			case MetricType.AggregatedAverageDiskThroughput:
				AggregatedAverageNormalizedIOPS = value.IntegerValue;
				break;
			case MetricType.AggregatedMaximumDiskAllocation:
				TotalDiskAllocation = value.IntegerValue;
				break;
			case MetricType.AggregatedDiskNormalizedIOCount:
				AggregatedNormalizedIOCount = value.IntegerValue;
				break;
			case MetricType.AggregatedDiskDataWritten:
				AggregatedDiskDataWritten = value.IntegerValue;
				break;
			case MetricType.AggregatedDiskDataRead:
				AggregatedDiskDataRead = value.IntegerValue;
				break;
			}
		}
		UpdateMeteringDuration(values);
	}
}

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMMeteringReportForResourcePool : VMResourceReport
{
    private IReadOnlyDictionary<VMResourcePoolType, IMeasurableResourcePool> m_ResourcePools;

    public string ResourcePoolName
    {
        get
        {
            KeyValuePair<VMResourcePoolType, IMeasurableResourcePool> keyValuePair = m_ResourcePools.FirstOrDefault();
            if (keyValuePair.Value != null)
            {
                return keyValuePair.Value.Name;
            }
            return string.Empty;
        }
    }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    public VMResourcePoolType[] ResourcePoolType => m_ResourcePools.Keys.ToArray();

    internal VMMeteringReportForResourcePool(Server server, IReadOnlyDictionary<VMResourcePoolType, IMeasurableResourcePool> resourcePools)
        : base(server)
    {
        if (resourcePools == null)
        {
            throw new ArgumentNullException("resourcePools");
        }
        m_ResourcePools = resourcePools;
        List<IMetricValue> values = m_ResourcePools.SelectMany((KeyValuePair<VMResourcePoolType, IMeasurableResourcePool> entry) => entry.Value.GetMetricValues()).ToList();
        ParseMetricValues(values);
        ParsePortAclMetricValues(values);
        if (m_ResourcePools.TryGetValue(VMResourcePoolType.VHD, out var value))
        {
            VMVhdResourcePool vMVhdResourcePool = (VMVhdResourcePool)value;
            base.HardDiskMetrics = (from hardDiskDrive in vMVhdResourcePool.GetAllocatedHardDiskDrives()
                select new VHDMetrics(hardDiskDrive)).ToArray();
        }
    }

    private void ParsePortAclMetricValues(IEnumerable<IMetricValue> values)
    {
        VMPortAclMeteringReport[] networkMeteredTrafficReport = (from value in values.Where(IsSupportedPortAclMetric)
            select new VMResourcePoolPortAclMeteringReport(value.BreakdownValue, (value.MetricType == MetricType.AggregatedIncomingNetworkTraffic) ? VMNetworkAdapterAclDirection.Inbound : VMNetworkAdapterAclDirection.Outbound, value.IntegerValue)).ToArray();
        base.NetworkMeteredTrafficReport = networkMeteredTrafficReport;
    }

    private static bool IsSupportedPortAclMetric(IMetricValue value)
    {
        if (value.MetricType != MetricType.AggregatedIncomingNetworkTraffic)
        {
            return value.MetricType == MetricType.AggregatedOutgoingNetworkTraffic;
        }
        return true;
    }
}

using System.Collections.Generic;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal class VHDMetrics
{
    public HardDiskDrive VirtualHardDisk { get; private set; }

    public int? AverageNormalizedIOPS { get; private set; }

    public int? AverageLatency { get; private set; }

    public int? DataRead { get; private set; }

    public int? DataWritten { get; private set; }

    public int? NormalizedIOCount { get; private set; }

    internal VHDMetrics(HardDiskDrive virtualHardDisk)
    {
        VirtualHardDisk = virtualHardDisk;
        IReadOnlyCollection<IMetricValue> metricValues = virtualHardDisk.GetMetricValues();
        if (metricValues == null)
        {
            return;
        }
        foreach (IMetricValue item in metricValues)
        {
            switch (item.MetricType)
            {
            case MetricType.AverageDiskThroughput:
                AverageNormalizedIOPS = NumberConverter.UInt64ToInt32(item.IntegerValue);
                break;
            case MetricType.AverageDiskLatency:
                AverageLatency = NumberConverter.UInt64ToInt32(item.IntegerValue);
                break;
            case MetricType.DiskDataRead:
                DataRead = NumberConverter.UInt64ToInt32(item.IntegerValue);
                break;
            case MetricType.DiskDataWritten:
                DataWritten = NumberConverter.UInt64ToInt32(item.IntegerValue);
                break;
            case MetricType.DiskNormalizedIOCount:
                NormalizedIOCount = NumberConverter.UInt64ToInt32(item.IntegerValue);
                break;
            }
        }
    }
}

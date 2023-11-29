#define TRACE
using System.Globalization;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class MetricDefinitionView : View, IMetricDefinition, IVirtualizationManagementObject
{
    private static class MetricIds
    {
        public const string AverageMemory = "Microsoft:04BDF59E-580D-4441-8828-FFFE44472D2D";

        public const string MaximumMemory = "Microsoft:394DCE66-458F-4895-AE56-41D7C9602A49";

        public const string MinimumMemory = "Microsoft:FF85EA46-9933-4436-BE5D-C96827399966";

        public const string AverageCpu = "Microsoft:3F6F1051-C8FC-47EF-9821-C07240848748";

        public const string AverageDiskLatency = "Microsoft:72544D55-3035-41DA-B9D7-6C5A39BF8F35";

        public const string AverageDiskThroughput = "Microsoft:A4BCA0D9-C27D-4BC8-A7E3-7ED13C89E373";

        public const string MaximumDiskAllocation = "Microsoft:AD29978B-AAB6-44AE-81CD-0609BF929F18";

        public const string DiskNormalizedIOCount = "Microsoft:4E1D459F-7861-46A4-887C-B64397C97E1B";

        public const string DiskDataWritten = "Microsoft:534FA8D7-9875-4FAB-BAA6-2424DF29B31E";

        public const string DiskDataRead = "Microsoft:A9DFBC22-E05F-438D-9405-22E2078353D6";

        public const string OutgoingNetworkTraffic = "Microsoft:3C215D49-078B-4D9F-8E2E-0C844642D3BB";

        public const string IncomingNetworkTraffic = "Microsoft:A6C250B0-867B-4CE7-9C47-A4F00AA6BB15";

        public const string AggregatedAverageMemory = "Microsoft:04BDF59E-580D-4441-8828-FFFE44472D2E";

        public const string AggregatedMaximumMemory = "Microsoft:394DCE66-458F-4895-AE56-41D7C9602A4A";

        public const string AggregatedMinimumMemory = "Microsoft:FF85EA46-9933-4436-BE5D-C96827399967";

        public const string AggregatedAverageCpu = "Microsoft:3F6F1051-C8FC-47EF-9821-C07240848749";

        public const string AggregatedAverageDiskLatency = "Microsoft:8F5001D9-CAB4-4F85-AC1E-887FBBB07641";

        public const string AggregatedAverageDiskThroughput = "Microsoft:7A9EB0A9-28CF-4216-A6CA-F8DE74A5D4A3";

        public const string AggregatedMaximumDiskAllocation = "Microsoft:AD29978B-AAB6-44AE-81CD-0609BF929F19";

        public const string AggregatedDiskNormalizedIOCount = "Microsoft:DF735A3F-AB45-4368-80CC-E06F73E8A85C";

        public const string AggregatedDiskDataWritten = "Microsoft:FFD2C1DC-091E-4F2A-95F7-75994CE92046";

        public const string AggregatedDiskDataRead = "Microsoft:942103D7-A125-4F19-B734-97AEDBF81995";

        public const string AggregatedOutgoingNetworkTraffic = "Microsoft:3C215D49-078B-4D9F-8E2E-0C844642D3BC";

        public const string AggregatedIncomingNetworkTraffic = "Microsoft:A6C250B0-867B-4CE7-9C47-A4F00AA6BB16";
    }

    public static MetricType MapMetricIdToMetricType(string metricId)
    {
        switch (metricId)
        {
        case "Microsoft:04BDF59E-580D-4441-8828-FFFE44472D2D":
            return MetricType.AverageMemory;
        case "Microsoft:394DCE66-458F-4895-AE56-41D7C9602A49":
            return MetricType.MaximumMemory;
        case "Microsoft:FF85EA46-9933-4436-BE5D-C96827399966":
            return MetricType.MinimumMemory;
        case "Microsoft:3F6F1051-C8FC-47EF-9821-C07240848748":
            return MetricType.AverageCpu;
        case "Microsoft:72544D55-3035-41DA-B9D7-6C5A39BF8F35":
            return MetricType.AverageDiskLatency;
        case "Microsoft:A4BCA0D9-C27D-4BC8-A7E3-7ED13C89E373":
            return MetricType.AverageDiskThroughput;
        case "Microsoft:AD29978B-AAB6-44AE-81CD-0609BF929F18":
            return MetricType.MaximumDiskAllocation;
        case "Microsoft:4E1D459F-7861-46A4-887C-B64397C97E1B":
            return MetricType.DiskNormalizedIOCount;
        case "Microsoft:534FA8D7-9875-4FAB-BAA6-2424DF29B31E":
            return MetricType.DiskDataWritten;
        case "Microsoft:A9DFBC22-E05F-438D-9405-22E2078353D6":
            return MetricType.DiskDataRead;
        case "Microsoft:3C215D49-078B-4D9F-8E2E-0C844642D3BB":
            return MetricType.OutgoingNetworkTraffic;
        case "Microsoft:A6C250B0-867B-4CE7-9C47-A4F00AA6BB15":
            return MetricType.IncomingNetworkTraffic;
        case "Microsoft:04BDF59E-580D-4441-8828-FFFE44472D2E":
            return MetricType.AggregatedAverageMemory;
        case "Microsoft:394DCE66-458F-4895-AE56-41D7C9602A4A":
            return MetricType.AggregatedMaximumMemory;
        case "Microsoft:FF85EA46-9933-4436-BE5D-C96827399967":
            return MetricType.AggregatedMinimumMemory;
        case "Microsoft:3F6F1051-C8FC-47EF-9821-C07240848749":
            return MetricType.AggregatedAverageCpu;
        case "Microsoft:8F5001D9-CAB4-4F85-AC1E-887FBBB07641":
            return MetricType.AggregatedAverageDiskLatency;
        case "Microsoft:7A9EB0A9-28CF-4216-A6CA-F8DE74A5D4A3":
            return MetricType.AggregatedAverageDiskThroughput;
        case "Microsoft:AD29978B-AAB6-44AE-81CD-0609BF929F19":
            return MetricType.AggregatedMaximumDiskAllocation;
        case "Microsoft:DF735A3F-AB45-4368-80CC-E06F73E8A85C":
            return MetricType.AggregatedDiskNormalizedIOCount;
        case "Microsoft:FFD2C1DC-091E-4F2A-95F7-75994CE92046":
            return MetricType.AggregatedDiskDataWritten;
        case "Microsoft:942103D7-A125-4F19-B734-97AEDBF81995":
            return MetricType.AggregatedDiskDataRead;
        case "Microsoft:3C215D49-078B-4D9F-8E2E-0C844642D3BC":
            return MetricType.AggregatedOutgoingNetworkTraffic;
        case "Microsoft:A6C250B0-867B-4CE7-9C47-A4F00AA6BB16":
            return MetricType.AggregatedIncomingNetworkTraffic;
        default:
            VMTrace.TraceWarning(string.Format(CultureInfo.InvariantCulture, "Metric definition identifier {0} is not a known metric definition.", metricId));
            return MetricType.Unknown;
        }
    }
}

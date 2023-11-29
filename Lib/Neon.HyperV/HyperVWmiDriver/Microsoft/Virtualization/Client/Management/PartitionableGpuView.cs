namespace Microsoft.Virtualization.Client.Management;

internal class PartitionableGpuView : View, IPartitionableGpu, IVirtualizationManagementObject, IPutableAsync, IPutable
{
    internal static class WmiPropertyNames
    {
        public const string Name = "Name";

        public const string ValidPartitionCounts = "ValidPartitionCounts";

        public const string PartitionCount = "PartitionCount";

        public const string TotalVRAM = "TotalVRAM";

        public const string AvailableVRAM = "AvailableVRAM";

        public const string MinPartitionVRAM = "MinPartitionVRAM";

        public const string MaxPartitionVRAM = "MaxPartitionVRAM";

        public const string OptimalPartitionVRAM = "OptimalPartitionVRAM";

        public const string TotalEncode = "TotalEncode";

        public const string AvailableEncode = "AvailableEncode";

        public const string MinPartitionEncode = "MinPartitionEncode";

        public const string MaxPartitionEncode = "MaxPartitionEncode";

        public const string OptimalPartitionEncode = "OptimalPartitionEncode";

        public const string TotalDecode = "TotalDecode";

        public const string AvailableDecode = "AvailableDecode";

        public const string MinPartitionDecode = "MinPartitionDecode";

        public const string MaxPartitionDecode = "MaxPartitionDecode";

        public const string OptimalPartitionDecode = "OptimalPartitionDecode";

        public const string TotalCompute = "TotalCompute";

        public const string AvailableCompute = "AvailableCompute";

        public const string MinPartitionCompute = "MinPartitionCompute";

        public const string MaxPartitionCompute = "MaxPartitionCompute";

        public const string OptimalPartitionCompute = "OptimalPartitionCompute";
    }

    public string Name => GetProperty<string>("Name");

    public ushort[] ValidPartitionCounts => GetProperty<ushort[]>("ValidPartitionCounts");

    public ushort PartitionCount
    {
        get
        {
            return GetProperty<ushort>("PartitionCount");
        }
        set
        {
            SetProperty("PartitionCount", value);
        }
    }

    public ulong TotalVRAM => GetProperty<ulong>("TotalVRAM");

    public ulong AvailableVRAM => GetProperty<ulong>("AvailableVRAM");

    public ulong MinPartitionVRAM => GetProperty<ulong>("MinPartitionVRAM");

    public ulong MaxPartitionVRAM => GetProperty<ulong>("MaxPartitionVRAM");

    public ulong OptimalPartitionVRAM => GetProperty<ulong>("OptimalPartitionVRAM");

    public ulong TotalEncode => GetProperty<ulong>("TotalEncode");

    public ulong AvailableEncode => GetProperty<ulong>("AvailableEncode");

    public ulong MinPartitionEncode => GetProperty<ulong>("MinPartitionEncode");

    public ulong MaxPartitionEncode => GetProperty<ulong>("MaxPartitionEncode");

    public ulong OptimalPartitionEncode => GetProperty<ulong>("OptimalPartitionEncode");

    public ulong TotalDecode => GetProperty<ulong>("TotalDecode");

    public ulong AvailableDecode => GetProperty<ulong>("AvailableDecode");

    public ulong MinPartitionDecode => GetProperty<ulong>("MinPartitionDecode");

    public ulong MaxPartitionDecode => GetProperty<ulong>("MaxPartitionDecode");

    public ulong OptimalPartitionDecode => GetProperty<ulong>("OptimalPartitionDecode");

    public ulong TotalCompute => GetProperty<ulong>("TotalCompute");

    public ulong AvailableCompute => GetProperty<ulong>("AvailableCompute");

    public ulong MinPartitionCompute => GetProperty<ulong>("MinPartitionCompute");

    public ulong MaxPartitionCompute => GetProperty<ulong>("MaxPartitionCompute");

    public ulong OptimalPartitionCompute => GetProperty<ulong>("OptimalPartitionCompute");
}

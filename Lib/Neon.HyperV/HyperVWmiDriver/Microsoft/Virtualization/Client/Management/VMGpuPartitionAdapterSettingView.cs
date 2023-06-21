namespace Microsoft.Virtualization.Client.Management;

internal class VMGpuPartitionAdapterSettingView : VMDeviceSettingView, IVMGpuPartitionAdapterSetting, IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable, IDeleteableAsync, IDeleteable
{
    internal static class WmiPropertyNames
    {
        public const string MinPartitionVRAM = "MinPartitionVRAM";

        public const string MaxPartitionVRAM = "MaxPartitionVRAM";

        public const string OptimalPartitionVRAM = "OptimalPartitionVRAM";

        public const string MinPartitionEncode = "MinPartitionEncode";

        public const string MaxPartitionEncode = "MaxPartitionEncode";

        public const string OptimalPartitionEncode = "OptimalPartitionEncode";

        public const string MinPartitionDecode = "MinPartitionDecode";

        public const string MaxPartitionDecode = "MaxPartitionDecode";

        public const string OptimalPartitionDecode = "OptimalPartitionDecode";

        public const string MinPartitionCompute = "MinPartitionCompute";

        public const string MaxPartitionCompute = "MaxPartitionCompute";

        public const string OptimalPartitionCompute = "OptimalPartitionCompute";
    }

    public ulong? MinPartitionVRAM
    {
        get
        {
            return GetProperty<ulong?>("MinPartitionVRAM");
        }
        set
        {
            SetProperty("MinPartitionVRAM", value);
        }
    }

    public ulong? MaxPartitionVRAM
    {
        get
        {
            return GetProperty<ulong?>("MaxPartitionVRAM");
        }
        set
        {
            SetProperty("MaxPartitionVRAM", value);
        }
    }

    public ulong? OptimalPartitionVRAM
    {
        get
        {
            return GetProperty<ulong?>("OptimalPartitionVRAM");
        }
        set
        {
            SetProperty("OptimalPartitionVRAM", value);
        }
    }

    public ulong? MinPartitionEncode
    {
        get
        {
            return GetProperty<ulong?>("MinPartitionEncode");
        }
        set
        {
            SetProperty("MinPartitionEncode", value);
        }
    }

    public ulong? MaxPartitionEncode
    {
        get
        {
            return GetProperty<ulong?>("MaxPartitionEncode");
        }
        set
        {
            SetProperty("MaxPartitionEncode", value);
        }
    }

    public ulong? OptimalPartitionEncode
    {
        get
        {
            return GetProperty<ulong?>("OptimalPartitionEncode");
        }
        set
        {
            SetProperty("OptimalPartitionEncode", value);
        }
    }

    public ulong? MinPartitionDecode
    {
        get
        {
            return GetProperty<ulong?>("MinPartitionDecode");
        }
        set
        {
            SetProperty("MinPartitionDecode", value);
        }
    }

    public ulong? MaxPartitionDecode
    {
        get
        {
            return GetProperty<ulong?>("MaxPartitionDecode");
        }
        set
        {
            SetProperty("MaxPartitionDecode", value);
        }
    }

    public ulong? OptimalPartitionDecode
    {
        get
        {
            return GetProperty<ulong?>("OptimalPartitionDecode");
        }
        set
        {
            SetProperty("OptimalPartitionDecode", value);
        }
    }

    public ulong? MinPartitionCompute
    {
        get
        {
            return GetProperty<ulong?>("MinPartitionCompute");
        }
        set
        {
            SetProperty("MinPartitionCompute", value);
        }
    }

    public ulong? MaxPartitionCompute
    {
        get
        {
            return GetProperty<ulong?>("MaxPartitionCompute");
        }
        set
        {
            SetProperty("MaxPartitionCompute", value);
        }
    }

    public ulong? OptimalPartitionCompute
    {
        get
        {
            return GetProperty<ulong?>("OptimalPartitionCompute");
        }
        set
        {
            SetProperty("OptimalPartitionCompute", value);
        }
    }
}

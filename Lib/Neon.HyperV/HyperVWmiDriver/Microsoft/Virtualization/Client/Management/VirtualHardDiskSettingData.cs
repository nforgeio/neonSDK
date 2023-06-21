using System;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_VirtualHardDiskSettingData")]
internal class VirtualHardDiskSettingData : EmbeddedInstance
{
    internal static class WmiPropertyNames
    {
        public const string DiskType = "Type";

        public const string DiskFormat = "Format";

        public const string Path = "Path";

        public const string ParentPath = "ParentPath";

        public const string MaxInternalSize = "MaxInternalSize";

        public const string BlockSize = "BlockSize";

        public const string LogicalSectorSize = "LogicalSectorSize";

        public const string PhysicalSectorSize = "PhysicalSectorSize";

        public const string VirtualDiskIdentifier = "VirtualDiskId";

        public const string IsPmemCompatible = "IsPmemCompatible";

        public const string PmemAddressAbstractionType = "PmemAddressAbstractionType";

        public const string DataAlignment = "DataAlignment";
    }

    public VirtualHardDiskType DiskType => (VirtualHardDiskType)GetProperty("Type", (ushort)0);

    public VirtualHardDiskFormat DiskFormat => (VirtualHardDiskFormat)GetProperty("Format", (ushort)0);

    public string Path => GetProperty<string>("Path");

    public string ParentPath => GetProperty<string>("ParentPath");

    public long MaxInternalSize
    {
        get
        {
            return NumberConverter.UInt64ToInt64(GetProperty("MaxInternalSize", 0uL));
        }
        set
        {
            SetProperty("MaxInternalSize", NumberConverter.Int64ToUInt64(value));
        }
    }

    public long BlockSize
    {
        get
        {
            return GetProperty("BlockSize", 0u);
        }
        set
        {
            SetProperty("BlockSize", NumberConverter.Int64ToUInt32(value));
        }
    }

    public long LogicalSectorSize
    {
        get
        {
            return GetProperty("LogicalSectorSize", 0u);
        }
        set
        {
            SetProperty("LogicalSectorSize", NumberConverter.Int64ToUInt32(value));
        }
    }

    public long PhysicalSectorSize
    {
        get
        {
            return GetProperty("PhysicalSectorSize", 0u);
        }
        set
        {
            SetProperty("PhysicalSectorSize", NumberConverter.Int64ToUInt32(value));
        }
    }

    public string VirtualDiskIdentifier
    {
        get
        {
            return GetProperty<string>("VirtualDiskId");
        }
        set
        {
            SetProperty("VirtualDiskId", value);
        }
    }

    public bool IsPmemCompatible
    {
        get
        {
            return GetProperty("IsPmemCompatible", defaultValue: false);
        }
        set
        {
            SetProperty("IsPmemCompatible", value);
        }
    }

    public VirtualHardDiskPmemAddressAbstractionType PmemAddressAbstractionType
    {
        get
        {
            return (VirtualHardDiskPmemAddressAbstractionType)GetProperty("PmemAddressAbstractionType", (ushort)0);
        }
        set
        {
            SetProperty("PmemAddressAbstractionType", NumberConverter.Int32ToUInt16((int)value));
        }
    }

    public long DataAlignment
    {
        get
        {
            return NumberConverter.UInt64ToInt64(GetProperty("DataAlignment", 0uL));
        }
        set
        {
            SetProperty("DataAlignment", NumberConverter.Int64ToUInt64(value));
        }
    }

    public VirtualHardDiskSettingData()
    {
    }

    public VirtualHardDiskSettingData(Server server, VirtualHardDiskType diskType, VirtualHardDiskFormat diskFormat, string path, string parentPath)
        : base(server, server.VirtualizationNamespace, "Msvm_VirtualHardDiskSettingData")
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentException(null, "path");
        }
        AddProperty("Type", (ushort)diskType);
        AddProperty("Format", (ushort)diskFormat);
        AddProperty("Path", path);
        AddProperty("ParentPath", parentPath);
    }
}

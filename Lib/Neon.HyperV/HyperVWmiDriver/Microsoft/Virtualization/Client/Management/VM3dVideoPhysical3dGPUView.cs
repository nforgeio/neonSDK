using System;

namespace Microsoft.Virtualization.Client.Management;

internal class VM3dVideoPhysical3dGPUView : VMDeviceView, IVM3dVideoPhysical3dGPU, IVMDevice, IVirtualizationManagementObject
{
    internal static class WmiPropertyNames
    {
        public const string Name = "Name";

        public const string EnabledForVirtualization = "EnabledForVirtualization";

        public const string CompatibleForVirtualization = "CompatibleForVirtualization";

        public const string GPUID = "GPUID";

        public const string InstanceID = "InstanceID";

        public const string AdapterIndexID = "AdapterIndexID";

        public const string DirectXVersion = "DirectXVersion";

        public const string PixelShaderVersion = "PixelShaderVersion";

        public const string DedicatedVideoMemory = "DedicatedVideoMemory";

        public const string DedicatedSystemMemory = "DedicatedSystemMemory";

        public const string SharedSystemMemory = "SharedSystemMemory";

        public const string AvailableVideoMemory = "AvailableVideoMemory";

        public const string TotalVideoMemory = "TotalVideoMemory";

        public const string DriverProvider = "DriverProvider";

        public const string DriverDate = "DriverDate";

        public const string DriverInstalled = "DriverInstalled";

        public const string DriverVersion = "DriverVersion";

        public const string DriverModelVersion = "DriverModelVersion";
    }

    public const string DriverModelVersionDefault = "-";

    public string Name => GetProperty<string>("Name");

    public bool EnabledForVirtualization => GetProperty<bool>("EnabledForVirtualization");

    public bool CompatibleForVirtualization => GetPropertyOrDefault("CompatibleForVirtualization", defaultValue: true);

    public string GPUID => GetProperty<string>("GPUID");

    public string InstanceID => GetProperty<string>("InstanceID");

    public long AdapterIndexID => NumberConverter.UInt64ToInt64(GetProperty<ulong>("AdapterIndexID"));

    public string DirectXVersion => GetProperty<string>("DirectXVersion");

    public string PixelShaderVersion => GetProperty<string>("PixelShaderVersion");

    public long DedicatedVideoMemory => NumberConverter.UInt64ToInt64(GetProperty<ulong>("DedicatedVideoMemory"));

    public long DedicatedSystemMemory => NumberConverter.UInt64ToInt64(GetProperty<ulong>("DedicatedSystemMemory"));

    public long SharedSystemMemory => NumberConverter.UInt64ToInt64(GetProperty<ulong>("SharedSystemMemory"));

    public long AvailableVideoMemory => NumberConverter.UInt64ToInt64(GetProperty<ulong>("AvailableVideoMemory"));

    public long TotalVideoMemory => NumberConverter.UInt64ToInt64(GetProperty<ulong>("TotalVideoMemory"));

    public string DriverProvider => GetProperty<string>("DriverProvider");

    public DateTime DriverDate => GetProperty<DateTime>("DriverDate");

    public DateTime DriverInstalled => GetProperty<DateTime>("DriverInstalled");

    public string DriverVersion => GetProperty<string>("DriverVersion");

    public string DriverModelVersion => GetPropertyOrDefault("DriverModelVersion", "-");
}

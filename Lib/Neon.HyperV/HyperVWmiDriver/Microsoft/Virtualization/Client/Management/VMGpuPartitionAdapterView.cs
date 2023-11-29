namespace Microsoft.Virtualization.Client.Management;

internal class VMGpuPartitionAdapterView : VMDeviceView, IVMGpuPartitionAdapter, IVMDevice, IVirtualizationManagementObject
{
    internal static class WmiPropertyNames
    {
        public const string DeviceInstancePath = "DeviceInstancePath";

        public const string PartitionId = "PartitionId";
    }

    public string DeviceInstancePath => GetProperty<string>("DeviceInstancePath");

    public ulong PartitionId => GetProperty<ushort>("PartitionId");
}

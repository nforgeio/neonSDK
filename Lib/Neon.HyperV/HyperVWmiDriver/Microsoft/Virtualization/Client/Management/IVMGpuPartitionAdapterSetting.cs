namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_GpuPartitionSettingData")]
internal interface IVMGpuPartitionAdapterSetting : IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable, IDeleteableAsync, IDeleteable
{
    ulong? MinPartitionVRAM { get; set; }

    ulong? MaxPartitionVRAM { get; set; }

    ulong? OptimalPartitionVRAM { get; set; }

    ulong? MinPartitionEncode { get; set; }

    ulong? MaxPartitionEncode { get; set; }

    ulong? OptimalPartitionEncode { get; set; }

    ulong? MinPartitionDecode { get; set; }

    ulong? MaxPartitionDecode { get; set; }

    ulong? OptimalPartitionDecode { get; set; }

    ulong? MinPartitionCompute { get; set; }

    ulong? MaxPartitionCompute { get; set; }

    ulong? OptimalPartitionCompute { get; set; }
}

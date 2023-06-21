namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_PartitionableGpu")]
internal interface IPartitionableGpu : IVirtualizationManagementObject, IPutableAsync, IPutable
{
    string Name { get; }

    ushort[] ValidPartitionCounts { get; }

    ushort PartitionCount { get; set; }

    ulong TotalVRAM { get; }

    ulong AvailableVRAM { get; }

    ulong MinPartitionVRAM { get; }

    ulong MaxPartitionVRAM { get; }

    ulong OptimalPartitionVRAM { get; }

    ulong TotalEncode { get; }

    ulong AvailableEncode { get; }

    ulong MinPartitionEncode { get; }

    ulong MaxPartitionEncode { get; }

    ulong OptimalPartitionEncode { get; }

    ulong TotalDecode { get; }

    ulong AvailableDecode { get; }

    ulong MinPartitionDecode { get; }

    ulong MaxPartitionDecode { get; }

    ulong OptimalPartitionDecode { get; }

    ulong TotalCompute { get; }

    ulong AvailableCompute { get; }

    ulong MinPartitionCompute { get; }

    ulong MaxPartitionCompute { get; }

    ulong OptimalPartitionCompute { get; }
}

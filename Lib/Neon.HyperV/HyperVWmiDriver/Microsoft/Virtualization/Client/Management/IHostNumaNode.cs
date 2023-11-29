using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_NumaNode")]
internal interface IHostNumaNode : IVirtualizationManagementObject
{
    ulong CurrentlyConsumableMemoryBlocks { get; }

    string NodeId { get; }

    IVMMemory GetRelatedHostMemory();

    IEnumerable<IVMProcessor> GetRelatedHostProcessors();
}

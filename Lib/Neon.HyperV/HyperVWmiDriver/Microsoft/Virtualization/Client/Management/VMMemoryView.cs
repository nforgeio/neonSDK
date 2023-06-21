using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal class VMMemoryView : VMDeviceView, IVMMemory, IVMDevice, IVirtualizationManagementObject
{
    internal static class WmiPropertyNames
    {
        public const string NumberOfBlocks = "NumberOfBlocks";
    }

    public ulong AllocatedRam => GetProperty<ulong>("NumberOfBlocks");

    public IHostNumaNode GetNumaNode()
    {
        return GetRelatedObject<IHostNumaNode>(base.Associations.VMMemoryToNumaNode, throwIfNotFound: false);
    }

    public IEnumerable<IVMMemory> GetVirtualNumaNodes()
    {
        return GetRelatedObjects<IVMMemory>(base.Associations.AggregateMemoryToVirtualNumaNode);
    }

    public IEnumerable<IVMMemory> GetPhysicalMemory()
    {
        return GetRelatedObjects<IVMMemory>(base.Associations.VirtualMachineMemoryToHostMemory);
    }
}

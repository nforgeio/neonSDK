using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal class HostNumaNodeView : View, IHostNumaNode, IVirtualizationManagementObject
{
	private static class WmiMemberNames
	{
		internal const string CurrentlyConsumableMemoryBlocks = "CurrentlyConsumableMemoryBlocks";

		internal const string NodeId = "NodeID";
	}

	public ulong CurrentlyConsumableMemoryBlocks => GetProperty<ulong>("CurrentlyConsumableMemoryBlocks");

	public string NodeId => GetProperty<string>("NodeID");

	public IVMMemory GetRelatedHostMemory()
	{
		return GetRelatedObject<IVMMemory>(base.Associations.NumaNodeToHostMemory);
	}

	public IEnumerable<IVMProcessor> GetRelatedHostProcessors()
	{
		return GetRelatedObjects<IVMProcessor>(base.Associations.NumaNodeToHostProcessor);
	}
}

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Numa", Justification = "This is by spec.")]
internal sealed class VMHostNumaNode : VirtualizationObject
{
	private readonly DataUpdater<IVMMemory> m_HostMemory;

	private readonly DataUpdater<IHostNumaNode> m_HostNumaNode;

	[VirtualizationObjectIdentifier(IdentifierFlags.UniqueIdentifier | IdentifierFlags.FriendlyName)]
	public int NodeId
	{
		get
		{
			string nodeId = m_HostNumaNode.GetData(UpdatePolicy.None).NodeId;
			return int.Parse(nodeId.Substring(nodeId.LastIndexOf("\\", StringComparison.Ordinal) + 1), CultureInfo.InvariantCulture);
		}
	}

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	public int[] ProcessorsAvailability
	{
		get
		{
			List<IVMProcessor> list = m_HostNumaNode.GetData(UpdatePolicy.EnsureAssociatorsUpdated).GetRelatedHostProcessors().ToList();
			int[] array = new int[list.Count];
			foreach (IVMProcessor item in list)
			{
				int num = int.Parse(item.DeviceId.Substring(item.DeviceId.LastIndexOf('\\') + 1), CultureInfo.InvariantCulture);
				int num2 = (array[num] = 100 - item.LoadPercentage);
			}
			return array;
		}
	}

	public int MemoryAvailable
	{
		get
		{
			ulong currentlyConsumableMemoryBlocks = m_HostNumaNode.GetData(UpdatePolicy.EnsureUpdated).CurrentlyConsumableMemoryBlocks;
			if (currentlyConsumableMemoryBlocks <= int.MaxValue)
			{
				return NumberConverter.UInt64ToInt32(currentlyConsumableMemoryBlocks);
			}
			return int.MaxValue;
		}
	}

	public int MemoryTotal
	{
		get
		{
			ulong allocatedRam = m_HostMemory.GetData(UpdatePolicy.EnsureUpdated).AllocatedRam;
			if (allocatedRam <= int.MaxValue)
			{
				return NumberConverter.UInt64ToInt32(allocatedRam);
			}
			return int.MaxValue;
		}
	}

	private VMHostNumaNode(IHostNumaNode hostNumaNode)
		: base(hostNumaNode)
	{
		m_HostNumaNode = InitializePrimaryDataUpdater(hostNumaNode);
		m_HostMemory = new DataUpdater<IVMMemory>(m_HostNumaNode.GetData(UpdatePolicy.EnsureUpdated).GetRelatedHostMemory());
	}

	internal static VMHostNumaNode[] GetHostNumaNodes(Server server)
	{
		return (from node in ObjectLocator.GetHostNumaNodes(server)
			select new VMHostNumaNode(node)).ToArray();
	}
}

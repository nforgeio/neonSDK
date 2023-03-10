using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_Memory")]
internal interface IVMMemory : IVMDevice, IVirtualizationManagementObject
{
	ulong AllocatedRam { get; }

	IHostNumaNode GetNumaNode();

	IEnumerable<IVMMemory> GetVirtualNumaNodes();

	IEnumerable<IVMMemory> GetPhysicalMemory();
}

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Virtualization.Client.Management;

internal class VirtualDiskResourcePoolView : ResourcePoolView, IVirtualDiskResourcePool, IResourcePool, IVirtualizationManagementObject, IDeleteableAsync, IDeleteable, IMetricMeasurableElement
{
	public bool HasBaseOfStoragePath(string path)
	{
		if (base.Primordial)
		{
			return true;
		}
		return base.AllocationSettings.Cast<IVirtualDiskPoolAllocationSetting>().Any((IVirtualDiskPoolAllocationSetting setting) => setting.HasBaseOfStoragePath(path));
	}

	public List<string> GetStoragePaths()
	{
		return base.AllocationSettings.Cast<IVirtualDiskPoolAllocationSetting>().SelectMany((IVirtualDiskPoolAllocationSetting diskAllocation) => diskAllocation.GetStoragePaths()).ToList();
	}

	public IEnumerable<IVirtualDisk> GetAllocatedVirtualDisks()
	{
		return GetRelatedObjects<IVirtualDisk>(base.Associations.VirtualDiskAllocatedFromStoragePool);
	}
}

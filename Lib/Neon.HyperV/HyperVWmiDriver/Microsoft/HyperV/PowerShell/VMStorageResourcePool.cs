using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.HyperV.PowerShell.Common;
using Microsoft.Virtualization.Client.Management;

using ErrorMessages = Microsoft.HyperV.PowerShell.Common.ErrorMessages;

namespace Microsoft.HyperV.PowerShell;

[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
internal abstract class VMStorageResourcePool : VMResourcePool
{
	private static readonly IReadOnlyList<VMResourcePoolType> gm_StoragePoolTypes = new List<VMResourcePoolType>
	{
		VMResourcePoolType.VHD,
		VMResourcePoolType.ISO,
		VMResourcePoolType.VFD
	}.AsReadOnly();

	private static readonly IEqualityComparer<object> gm_StoragePathComparer = new PathEqualityComparer();

	internal static IReadOnlyList<VMResourcePoolType> Types => gm_StoragePoolTypes;

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
	public string[] Paths
	{
		get
		{
			IVirtualDiskResourcePool virtualDiskResourcePool = (IVirtualDiskResourcePool)m_ResourcePool.GetData(UpdatePolicy.EnsureAssociatorsUpdated);
			if (virtualDiskResourcePool.Primordial)
			{
				return virtualDiskResourcePool.ChildPools.Cast<IVirtualDiskResourcePool>().Select(VMResourcePool.NewVMResourcePool).Cast<VMStorageResourcePool>()
					.SelectMany((VMStorageResourcePool childStoragePool) => childStoragePool.GetStoragePaths())
					.Distinct(StringComparer.OrdinalIgnoreCase)
					.ToArray();
			}
			return GetStoragePaths().ToArray();
		}
	}

	protected override IEqualityComparer<object> HostResourceComparer => gm_StoragePathComparer;

	protected override string MissingResourcesError => ErrorMessages.VMStorageResourcePool_MissingResource;

	internal VMStorageResourcePool(IResourcePool resourcePool)
		: base(resourcePool)
	{
	}

	internal IEnumerable<string> GetStoragePaths()
	{
		return GetHostResources().Cast<string>();
	}

	internal void AddStoragePaths(IEnumerable<string> storagePathsToAdd, IOperationWatcher operationWatcher)
	{
		AddHostResources(storagePathsToAdd, TaskDescriptions.AddVMStoragePath, operationWatcher);
	}

	internal void RemoveStoragePaths(IReadOnlyList<string> storagePathToRemove, IOperationWatcher operationWatcher)
	{
		RemoveHostResources(storagePathToRemove, TaskDescriptions.RemoveVMStoragePath, operationWatcher);
	}

	internal IEnumerable<HardDiskDrive> GetAllocatedHardDiskDrives()
	{
		return (from virtualDisk in ((IVirtualDiskResourcePool)m_ResourcePool.GetData(UpdatePolicy.EnsureAssociatorsUpdated)).GetAllocatedVirtualDisks()
			select virtualDisk.VirtualDeviceSetting).Cast<IVirtualDiskSetting>().Select(HardDiskDrive.GetHardDiskDrive);
	}

	protected override IEnumerable<object> GetHostResources(IResourcePoolAllocationSetting poolAllocationSetting)
	{
		IVirtualDiskPoolAllocationSetting obj = (IVirtualDiskPoolAllocationSetting)poolAllocationSetting;
		obj.UpdatePropertyCache(Constants.UpdateThreshold);
		return obj.GetStoragePaths().ToList();
	}

	protected override bool HasHostResource(IResourcePoolAllocationSetting poolAllocationSetting, object hostResource)
	{
		IVirtualDiskPoolAllocationSetting obj = (IVirtualDiskPoolAllocationSetting)poolAllocationSetting;
		string value = (string)hostResource;
		obj.UpdatePropertyCache(Constants.UpdateThreshold);
		return obj.GetStoragePaths().Contains(value, StringComparer.OrdinalIgnoreCase);
	}

	protected override void SetHostResourceInAllocationFromParentPool(IEnumerable<object> hostResources, IResourcePool parentPool, IResourcePoolAllocationSetting parentPoolAllocationSetting)
	{
		if (hostResources == null)
		{
			throw new ArgumentNullException("hostResources");
		}
		IEnumerable<string> source = hostResources.Cast<string>();
		IVirtualDiskResourcePool virtualDiskResourcePool = (IVirtualDiskResourcePool)parentPool;
		IVirtualDiskPoolAllocationSetting virtualDiskPoolAllocationSetting = (IVirtualDiskPoolAllocationSetting)parentPoolAllocationSetting;
		if (virtualDiskResourcePool.Primordial)
		{
			virtualDiskPoolAllocationSetting.SetStoragePaths(source.ToList());
		}
		else
		{
			virtualDiskPoolAllocationSetting.SetStoragePaths(source.Where(virtualDiskResourcePool.HasBaseOfStoragePath).ToList());
		}
	}
}

using System;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IResourcePoolConfigurationServiceContract : IResourcePoolConfigurationService, IVirtualizationManagementObject
{
	public abstract Server Server { get; }

	public abstract WmiObjectPath ManagementPath { get; }

	public event EventHandler Deleted;

	public event EventHandler CacheUpdated;

	public IResourcePoolSetting CreateTemplatePoolSetting(string poolId, VMDeviceSettingType deviceType)
	{
		return null;
	}

	public IResourcePoolAllocationSetting CreateTemplateAllocationSetting(string poolId, VMDeviceSettingType deviceType)
	{
		return null;
	}

	public IVMTask BeginCreateResourcePool(IResourcePoolSetting resourcePoolSettingData, IResourcePool[] parentPools, IResourcePoolAllocationSetting[] resourceSettings)
	{
		return null;
	}

	public IResourcePool EndCreateResourcePool(IVMTask task)
	{
		return null;
	}

	public IVMTask BeginModifyResourcePool(IResourcePool childPool, IResourcePool[] parentPools, IResourcePoolAllocationSetting[] resourceSettings)
	{
		return null;
	}

	public void EndModifyResourcePool(IVMTask task)
	{
	}

	public abstract void InvalidatePropertyCache();

	public abstract void UpdatePropertyCache();

	public abstract void UpdatePropertyCache(TimeSpan threshold);

	public abstract void RegisterForInstanceModificationEvents(InstanceModificationEventStrategy strategy);

	public abstract void UnregisterForInstanceModificationEvents();

	public abstract void InvalidateAssociationCache();

	public abstract void UpdateAssociationCache();

	public abstract void UpdateAssociationCache(TimeSpan threshold);

	public abstract string GetEmbeddedInstance();

	public abstract void DiscardPendingPropertyChanges();
}
[WmiName("Msvm_ResourcePoolConfigurationService")]
internal interface IResourcePoolConfigurationService : IVirtualizationManagementObject
{
	IResourcePoolSetting CreateTemplatePoolSetting(string poolId, VMDeviceSettingType deviceType);

	IResourcePoolAllocationSetting CreateTemplateAllocationSetting(string poolId, VMDeviceSettingType deviceType);

	IVMTask BeginCreateResourcePool(IResourcePoolSetting resourcePoolSettingData, IResourcePool[] parentPools, IResourcePoolAllocationSetting[] resourceSettings);

	IResourcePool EndCreateResourcePool(IVMTask task);

	IVMTask BeginModifyResourcePool(IResourcePool childPool, IResourcePool[] parentPools, IResourcePoolAllocationSetting[] resourceSettings);

	void EndModifyResourcePool(IVMTask task);
}

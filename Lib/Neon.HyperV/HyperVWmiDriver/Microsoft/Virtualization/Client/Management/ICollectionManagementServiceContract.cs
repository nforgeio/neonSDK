using System;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class ICollectionManagementServiceContract : ICollectionManagementService, IVirtualizationManagementObject
{
	public abstract Server Server { get; }

	public abstract WmiObjectPath ManagementPath { get; }

	public event EventHandler Deleted;

	public event EventHandler CacheUpdated;

	public IVMTask BeginCreateCollection(string name, Guid instanceId, CollectionType type)
	{
		return null;
	}

	public IHyperVCollection EndCreateCollection(IVMTask task)
	{
		return null;
	}

	public IVMTask BeginRemoveMemberById(IHyperVCollection member, string collectionId)
	{
		return null;
	}

	public IVMTask BeginRemoveMemberById(IVMComputerSystemBase member, string collectionId)
	{
		return null;
	}

	public void EndRemoveMemberById(IVMTask task)
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

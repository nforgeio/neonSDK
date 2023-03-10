using System;
using Microsoft.Virtualization.Client.Management.Clustering;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IMSClusterVMOrConfigurationResourceContract : IMSClusterVMOrConfigurationResource, IMSClusterResource, IMSClusterResourceBase, IVirtualizationManagementObject
{
	public string VMComputerSystemInstanceId => null;

	public abstract string Name { get; }

	public abstract string Owner { get; }

	public abstract Server Server { get; }

	public abstract WmiObjectPath ManagementPath { get; }

	public event EventHandler Deleted;

	public event EventHandler CacheUpdated;

	public abstract IMSClusterResourceGroup GetGroup();

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

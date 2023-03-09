using System;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IVirtualizationManagementObjectContract : IVirtualizationManagementObject
{
	public Server Server => null;

	public WmiObjectPath ManagementPath => null;

	public event EventHandler Deleted;

	public event EventHandler CacheUpdated;

	public void InvalidatePropertyCache()
	{
	}

	public void UpdatePropertyCache()
	{
	}

	public void UpdatePropertyCache(TimeSpan threshold)
	{
	}

	public void RegisterForInstanceModificationEvents(InstanceModificationEventStrategy strategy)
	{
	}

	public void UnregisterForInstanceModificationEvents()
	{
	}

	public void InvalidateAssociationCache()
	{
	}

	public void UpdateAssociationCache()
	{
	}

	public void UpdateAssociationCache(TimeSpan threshold)
	{
	}

	public string GetEmbeddedInstance()
	{
		return null;
	}

	public void DiscardPendingPropertyChanges()
	{
	}
}
internal interface IVirtualizationManagementObject
{
	Server Server { get; }

	WmiObjectPath ManagementPath { get; }

	event EventHandler Deleted;

	event EventHandler CacheUpdated;

	void InvalidatePropertyCache();

	void UpdatePropertyCache();

	void UpdatePropertyCache(TimeSpan threshold);

	void RegisterForInstanceModificationEvents(InstanceModificationEventStrategy strategy);

	void UnregisterForInstanceModificationEvents();

	void InvalidateAssociationCache();

	void UpdateAssociationCache();

	void UpdateAssociationCache(TimeSpan threshold);

	string GetEmbeddedInstance();

	void DiscardPendingPropertyChanges();
}

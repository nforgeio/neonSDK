using System;
using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IEthernetSwitchExtensionContract : IEthernetSwitchExtension, IPutableAsync, IPutable, IEthernetSwitchExtensionBase, IVirtualizationManagementObject
{
	public bool IsChild => false;

	public bool IsEnabled
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public bool IsRunning => false;

	public IEnumerable<IEthernetSwitchExtension> Children => null;

	public IVirtualEthernetSwitch Switch => null;

	public IEthernetSwitchExtension Parent => null;

	public abstract Server Server { get; }

	public abstract WmiObjectPath ManagementPath { get; }

	public abstract string ExtensionId { get; }

	public abstract string FriendlyName { get; }

	public abstract EthernetSwitchExtensionType ExtensionType { get; }

	public abstract string Company { get; }

	public abstract string Version { get; }

	public abstract string Description { get; }

	public event EventHandler Deleted;

	public event EventHandler CacheUpdated;

	public abstract IVMTask BeginPut();

	public abstract void EndPut(IVMTask putTask);

	public abstract void Put();

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

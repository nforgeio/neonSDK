using System;
using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IVirtualEthernetSwitchContract : IVirtualEthernetSwitch, IVirtualSwitch, IVirtualizationManagementObject, IPutable
{
	public IVirtualEthernetSwitchSetting Setting => null;

	public IEnumerable<IVirtualEthernetSwitchPort> SwitchPorts => null;

	public bool IsDefaultSwitch => false;

	public IEthernetSwitchOffloadStatus OffloadStatus => null;

	public IEthernetSwitchBandwidthStatus BandwidthStatus => null;

	public abstract string InstanceId { get; }

	public abstract string FriendlyName { get; }

	public abstract Server Server { get; }

	public abstract WmiObjectPath ManagementPath { get; }

	public event EventHandler Deleted;

	public event EventHandler CacheUpdated;

	public IEnumerable<IEthernetSwitchStatus> GetRuntimeStatuses()
	{
		return null;
	}

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

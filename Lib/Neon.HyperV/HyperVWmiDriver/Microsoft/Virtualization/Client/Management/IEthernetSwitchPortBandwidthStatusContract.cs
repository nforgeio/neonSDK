using System;
using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IEthernetSwitchPortBandwidthStatusContract : IEthernetSwitchPortBandwidthStatus, IEthernetPortStatus, IEthernetStatus, IVirtualizationManagementObject
{
	public uint CurrentBandwidthReservationPercentage => 0u;

	public abstract string Name { get; }

	public abstract string ExtensionId { get; }

	public abstract string FeatureId { get; }

	public abstract IReadOnlyDictionary<string, object> Properties { get; }

	public abstract Server Server { get; }

	public abstract WmiObjectPath ManagementPath { get; }

	public event EventHandler Deleted;

	public event EventHandler CacheUpdated;

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

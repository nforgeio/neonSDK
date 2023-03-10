using System;
using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IEthernetSwitchOffloadStatusContract : IEthernetSwitchOffloadStatus, IEthernetSwitchStatus, IEthernetStatus, IVirtualizationManagementObject
{
	public uint IovQueuePairCapacity => 0u;

	public uint IovQueuePairUsage => 0u;

	public uint IovVfCapacity => 0u;

	public uint IovVfUsage => 0u;

	public uint IPsecSACapacity => 0u;

	public uint IPsecSAUsage => 0u;

	public uint VmqCapacity => 0u;

	public uint VmqUsage => 0u;

	public bool PacketDirectInUse => false;

	public bool DefaultQueueVrssEnabled => false;

	public bool DefaultQueueVmmqEnabled => false;

	public uint DefaultQueueVmmqQueuePairs => 0u;

	public uint DefaultQueueVrssMinQueuePairs => 0u;

	public uint DefaultQueueVrssQueueSchedulingMode => 0u;

	public bool DefaultQueueVrssExcludePrimaryProcessor => false;

	public bool DefaultQueueVrssIndependentHostSpreading => false;

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

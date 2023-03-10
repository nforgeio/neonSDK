using System;
using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IEthernetSwitchPortOffloadStatusContract : IEthernetSwitchPortOffloadStatus, IEthernetPortStatus, IEthernetStatus, IVirtualizationManagementObject
{
	public int VmqOffloadUsage => 0;

	public int IovOffloadUsage => 0;

	public uint IovQueuePairUsage => 0u;

	public bool IovOffloadActive => false;

	public uint IpsecCurrentOffloadSaCount => 0u;

	public uint VmqId => 0u;

	public ushort IovVirtualFunctionId => 0;

	public bool VrssEnabled => false;

	public bool VmmqEnabled => false;

	public uint VmmqQueuePairs => 0u;

	public uint VrssMinQueuePairs => 0u;

	public uint VrssQueueSchedulingMode => 0u;

	public bool VrssExcludePrimaryProcessor => false;

	public bool VrssIndependentHostSpreading => false;

	public uint VrssVmbusChannelAffinityPolicy => 0u;

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

using System;
using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IVirtualEthernetSwitchPortContract : IVirtualEthernetSwitchPort, IEthernetPort, IVirtualSwitchPort, IVirtualizationManagementObject
{
    public IVirtualEthernetSwitch VirtualEthernetSwitch => null;

    public IVirtualEthernetSwitchPortSetting Setting => null;

    public IEthernetSwitchPortOffloadStatus OffloadStatus => null;

    public IEthernetSwitchPortBandwidthStatus BandwidthStatus => null;

    public abstract string DeviceId { get; }

    public abstract string PermanentAddress { get; }

    public abstract ILanEndpoint LanEndpoint { get; }

    public abstract string Name { get; }

    public abstract string FriendlyName { get; }

    public abstract Server Server { get; }

    public abstract WmiObjectPath ManagementPath { get; }

    public event EventHandler Deleted;

    public event EventHandler CacheUpdated;

    public IEnumerable<IEthernetPortStatus> GetRuntimeStatuses()
    {
        return null;
    }

    public abstract IEthernetPort GetConnectedEthernetPort(TimeSpan threshold);

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

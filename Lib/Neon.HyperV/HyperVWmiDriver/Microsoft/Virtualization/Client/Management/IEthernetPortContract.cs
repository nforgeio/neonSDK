using System;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IEthernetPortContract : IEthernetPort, IVirtualSwitchPort, IVirtualizationManagementObject
{
    public string DeviceId => null;

    public string PermanentAddress => null;

    public ILanEndpoint LanEndpoint => null;

    public abstract string Name { get; }

    public abstract string FriendlyName { get; }

    public abstract Server Server { get; }

    public abstract WmiObjectPath ManagementPath { get; }

    public event EventHandler Deleted;

    public event EventHandler CacheUpdated;

    public IEthernetPort GetConnectedEthernetPort(TimeSpan threshold)
    {
        return null;
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

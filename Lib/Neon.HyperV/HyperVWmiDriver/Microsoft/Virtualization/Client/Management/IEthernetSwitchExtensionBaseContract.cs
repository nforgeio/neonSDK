using System;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IEthernetSwitchExtensionBaseContract : IEthernetSwitchExtensionBase, IVirtualizationManagementObject
{
    public string ExtensionId => null;

    public string FriendlyName => null;

    public EthernetSwitchExtensionType ExtensionType => EthernetSwitchExtensionType.Unknown;

    public string Company => null;

    public string Version => null;

    public string Description => null;

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

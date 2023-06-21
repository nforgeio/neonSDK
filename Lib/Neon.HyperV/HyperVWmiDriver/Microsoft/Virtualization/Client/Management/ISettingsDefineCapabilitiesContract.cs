using System;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class ISettingsDefineCapabilitiesContract : ISettingsDefineCapabilities, IVirtualizationManagementObject
{
    public IVirtualizationManagementObject PartComponent => null;

    public CapabilitiesValueRole ValueRole => CapabilitiesValueRole.Default;

    public CapabilitiesValueRange ValueRange => CapabilitiesValueRange.Point;

    public CapabilitiesSupportStatement SupportStatement => CapabilitiesSupportStatement.Production;

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

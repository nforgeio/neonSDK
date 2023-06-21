using System;

namespace Microsoft.Virtualization.Client.Management;

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

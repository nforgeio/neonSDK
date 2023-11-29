using System;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IDeleteableContract : IDeleteable
{
    public abstract Server Server { get; }

    public abstract WmiObjectPath ManagementPath { get; }

    public event EventHandler Deleted
    {
        add
        {
        }
        remove
        {
        }
    }

    public event EventHandler CacheUpdated
    {
        add
        {
        }
        remove
        {
        }
    }

    public void Delete()
    {
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

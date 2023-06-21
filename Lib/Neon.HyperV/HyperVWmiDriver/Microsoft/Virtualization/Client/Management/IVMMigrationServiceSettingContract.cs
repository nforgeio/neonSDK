using System;
using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IVMMigrationServiceSettingContract : IVMMigrationServiceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable
{
    public bool EnableVirtualSystemMigration
    {
        get
        {
            return false;
        }
        set
        {
        }
    }

    public long MaximumActiveVirtualSystemMigration
    {
        get
        {
            return 0L;
        }
        set
        {
        }
    }

    public long MaximumActiveStorageMigration
    {
        get
        {
            return 0L;
        }
        set
        {
        }
    }

    public int AuthenticationType
    {
        get
        {
            return 0;
        }
        set
        {
        }
    }

    public bool EnableCompression
    {
        get
        {
            return false;
        }
        set
        {
        }
    }

    public bool EnableSmbTransport
    {
        get
        {
            return false;
        }
        set
        {
        }
    }

    public bool SmbTransportOptionAvailable => false;

    public bool CompressionOptionAvailable => false;

    public IEnumerable<IVMMigrationNetworkSetting> NetworkSettings => null;

    public abstract Server Server { get; }

    public abstract WmiObjectPath ManagementPath { get; }

    public event EventHandler Deleted;

    public event EventHandler CacheUpdated;

    public IEnumerable<IVMMigrationNetworkSetting> GetUserManagedNetworkSettings()
    {
        return null;
    }

    public abstract IVMTask BeginPut();

    public abstract void EndPut(IVMTask putTask);

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

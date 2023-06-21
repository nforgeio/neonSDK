using System;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IWin32DiskDriveContract : IWin32DiskDrive, IVirtualizationManagementObject
{
    public uint DiskNumber => 0u;

    public ushort LunId => 0;

    public uint PathId => 0u;

    public ushort PortNumber => 0;

    public ushort TargetId => 0;

    public string DeviceId => null;

    public abstract Server Server { get; }

    public abstract WmiObjectPath ManagementPath { get; }

    public event EventHandler Deleted;

    public event EventHandler CacheUpdated;

    public IMountedStorageImage GetMountedStorageImage()
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

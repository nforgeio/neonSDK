using System;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IMountedStorageImageContract : IMountedStorageImage, IVirtualizationManagementObject
{
    public string ImagePath => null;

    public byte LunId => 0;

    public byte PathId => 0;

    public byte PortNumber => 0;

    public byte TargetId => 0;

    public abstract Server Server { get; }

    public abstract WmiObjectPath ManagementPath { get; }

    public event EventHandler Deleted;

    public event EventHandler CacheUpdated;

    public IWin32DiskDrive GetDiskDrive()
    {
        return null;
    }

    public IVMTask BeginDetachVirtualHardDisk()
    {
        return null;
    }

    public void EndDetachVirtualHardDisk(IVMTask task)
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

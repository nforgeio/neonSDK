using System;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IAssignableDeviceServiceContract : IAssignableDeviceService, IVirtualizationManagementObject
{
    public abstract Server Server { get; }

    public abstract WmiObjectPath ManagementPath { get; }

    public event EventHandler Deleted;

    public event EventHandler CacheUpdated;

    public void DismountAssignableDevice(VMDismountSetting dismountSettingData, out string newDeviceInstanceId)
    {
        newDeviceInstanceId = null;
    }

    public void MountAssignableDevice(string instanceId, string locationPath, out string newDeviceInstanceId)
    {
        newDeviceInstanceId = null;
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

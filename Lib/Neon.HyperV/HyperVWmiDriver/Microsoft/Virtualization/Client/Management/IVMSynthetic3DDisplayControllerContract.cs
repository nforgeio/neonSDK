using System;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IVMSynthetic3DDisplayControllerContract : IVMSynthetic3DDisplayController, IVMDevice, IVirtualizationManagementObject
{
    public string AllocatedGPU => null;

    public abstract string FriendlyName { get; }

    public abstract string DeviceId { get; }

    public abstract IVMComputerSystem VirtualComputerSystem { get; }

    public abstract IVMDeviceSetting VirtualDeviceSetting { get; }

    public abstract Server Server { get; }

    public abstract WmiObjectPath ManagementPath { get; }

    public event EventHandler Deleted;

    public event EventHandler CacheUpdated;

    public IVM3dVideoPhysical3dGPU GetPhysical3dGraphicsProcessor()
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

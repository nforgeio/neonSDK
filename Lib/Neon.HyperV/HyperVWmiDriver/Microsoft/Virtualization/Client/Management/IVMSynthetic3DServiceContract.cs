using System;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IVMSynthetic3DServiceContract : IVMSynthetic3DService, IVirtualizationManagementObject
{
    public abstract Server Server { get; }

    public abstract WmiObjectPath ManagementPath { get; }

    public event EventHandler Deleted;

    public event EventHandler CacheUpdated;

    public IVMTask BeginEnableGPUForVirtualization(IVM3dVideoPhysical3dGPU physicalGPU)
    {
        return null;
    }

    public void EndEnableGPUForVirtualization(IVMTask task)
    {
    }

    public IVMTask BeginDisableGPUForVirtualization(IVM3dVideoPhysical3dGPU physicalGPU)
    {
        return null;
    }

    public void EndDisableGPUForVirtualization(IVMTask task)
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

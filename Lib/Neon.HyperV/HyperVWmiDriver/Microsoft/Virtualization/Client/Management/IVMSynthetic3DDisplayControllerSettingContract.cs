using System;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IVMSynthetic3DDisplayControllerSettingContract : IVMSynthetic3DDisplayControllerSetting, IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable, IDeleteableAsync, IDeleteable
{
    public int MaximumScreenResolution
    {
        get
        {
            return 0;
        }
        set
        {
        }
    }

    public int MaximumMonitors
    {
        get
        {
            return 0;
        }
        set
        {
        }
    }

    public ulong VRAMSizeBytes
    {
        get
        {
            return 0uL;
        }
        set
        {
        }
    }

    public abstract string FriendlyName { get; set; }

    public abstract string DeviceTypeName { get; }

    public abstract string DeviceId { get; }

    public abstract string PoolId { get; set; }

    public abstract Guid VMBusChannelInstanceGuid { get; set; }

    public abstract VMDeviceSettingType VMDeviceSettingType { get; }

    public abstract IVMDevice VirtualDevice { get; }

    public abstract IVMComputerSystemSetting VirtualComputerSystemSetting { get; }

    public abstract Server Server { get; }

    public abstract WmiObjectPath ManagementPath { get; }

    public event EventHandler Deleted;

    public event EventHandler CacheUpdated;

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

    public abstract IVMTask BeginDelete();

    public abstract void EndDelete(IVMTask deleteTask);

    public abstract void Delete();
}

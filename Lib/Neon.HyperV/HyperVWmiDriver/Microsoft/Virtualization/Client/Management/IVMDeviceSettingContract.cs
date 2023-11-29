using System;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IVMDeviceSettingContract : IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable
{
    public string FriendlyName
    {
        get
        {
            return null;
        }
        set
        {
        }
    }

    public string DeviceTypeName => null;

    public string DeviceId => null;

    public string PoolId
    {
        get
        {
            return null;
        }
        set
        {
        }
    }

    public Guid VMBusChannelInstanceGuid
    {
        get
        {
            return default(Guid);
        }
        set
        {
        }
    }

    public VMDeviceSettingType VMDeviceSettingType => VMDeviceSettingType.Unknown;

    public IVMDevice VirtualDevice => null;

    public IVMComputerSystemSetting VirtualComputerSystemSetting => null;

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
}

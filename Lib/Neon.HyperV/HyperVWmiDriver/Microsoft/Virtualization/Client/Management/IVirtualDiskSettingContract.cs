using System;
using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IVirtualDiskSettingContract : IVirtualDiskSetting, IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable, IDeleteableAsync, IDeleteable, IMetricMeasurableElement
{
    public string Path
    {
        get
        {
            return null;
        }
        set
        {
        }
    }

    public IVMDriveSetting DriveSetting
    {
        get
        {
            return null;
        }
        set
        {
        }
    }

    public IVirtualDiskResourcePool ResourcePool => null;

    public ulong MinimumIOPS
    {
        get
        {
            return 0uL;
        }
        set
        {
        }
    }

    public ulong MaximumIOPS
    {
        get
        {
            return 0uL;
        }
        set
        {
        }
    }

    public Guid StorageQoSPolicyID
    {
        get
        {
            return default(Guid);
        }
        set
        {
        }
    }

    public bool PersistentReservationsSupported
    {
        get
        {
            return false;
        }
        set
        {
        }
    }

    public ushort WriteHardeningMethod
    {
        get
        {
            return 0;
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

    public abstract MetricEnabledState AggregateMetricEnabledState { get; }

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

    public abstract IReadOnlyCollection<IMetricValue> GetMetricValues();
}

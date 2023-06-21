using System;
using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IVMMemorySettingContract : IVMMemorySetting, IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable, IMetricMeasurableElement
{
    public long AllocatedRam
    {
        get
        {
            return 0L;
        }
        set
        {
        }
    }

    public long MinimumMemory
    {
        get
        {
            return 0L;
        }
        set
        {
        }
    }

    public long MaximumMemory
    {
        get
        {
            return 0L;
        }
        set
        {
        }
    }

    public long MaximumMemoryPerNumaNode
    {
        get
        {
            return 0L;
        }
        set
        {
        }
    }

    public int PriorityLevel
    {
        get
        {
            return 0;
        }
        set
        {
        }
    }

    public int TargetMemoryBuffer
    {
        get
        {
            return 0;
        }
        set
        {
        }
    }

    public bool IsDynamicMemoryEnabled
    {
        get
        {
            return false;
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

    public abstract IReadOnlyCollection<IMetricValue> GetMetricValues();
}

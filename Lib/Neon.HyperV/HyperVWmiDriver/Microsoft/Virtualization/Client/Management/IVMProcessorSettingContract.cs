using System;
using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IVMProcessorSettingContract : IVMProcessorSetting, IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable, IMetricMeasurableElement
{
    public int Reservation
    {
        get
        {
            return 0;
        }
        set
        {
        }
    }

    public int VirtualQuantity
    {
        get
        {
            return 0;
        }
        set
        {
        }
    }

    public int Weight
    {
        get
        {
            return 0;
        }
        set
        {
        }
    }

    public long MaxProcessorsPerNumaNode
    {
        get
        {
            return 0L;
        }
        set
        {
        }
    }

    public long MaxNumaNodesPerSocket
    {
        get
        {
            return 0L;
        }
        set
        {
        }
    }

    public int Limit
    {
        get
        {
            return 0;
        }
        set
        {
        }
    }

    public bool LimitCpuId
    {
        get
        {
            return false;
        }
        set
        {
        }
    }

    public long? HwThreadsPerCore
    {
        get
        {
            return null;
        }
        set
        {
        }
    }

    public bool LimitProcessorFeatures
    {
        get
        {
            return false;
        }
        set
        {
        }
    }

    public bool EnableHostResourceProtection
    {
        get
        {
            return false;
        }
        set
        {
        }
    }

    public bool ExposeVirtualizationExtensions
    {
        get
        {
            return false;
        }
        set
        {
        }
    }

    public bool EnablePerfmonPmu
    {
        get
        {
            return false;
        }
        set
        {
        }
    }

    public bool EnablePerfmonLbr
    {
        get
        {
            return false;
        }
        set
        {
        }
    }

    public bool EnablePerfmonPebs
    {
        get
        {
            return false;
        }
        set
        {
        }
    }

    public bool EnablePerfmonIpt
    {
        get
        {
            return false;
        }
        set
        {
        }
    }

    public bool EnableLegacyApicMode
    {
        get
        {
            return false;
        }
        set
        {
        }
    }

    public bool AllowACountMCount
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

using System;
using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IResourcePoolContract : IResourcePool, IVirtualizationManagementObject, IDeleteableAsync, IDeleteable, IMetricMeasurableElement
{
    public string PoolId => null;

    public VMDeviceSettingType DeviceSettingType => VMDeviceSettingType.Unknown;

    public bool Primordial => false;

    public IResourcePoolSetting Setting => null;

    public IEnumerable<IVMDeviceSetting> AllCapabilities => null;

    public IEnumerable<IVMDevice> PhysicalDevices => null;

    public IEnumerable<IResourcePool> ParentPools => null;

    public IEnumerable<IResourcePool> ChildPools => null;

    public IEnumerable<IResourcePoolAllocationSetting> AllocationSettings => null;

    public abstract Server Server { get; }

    public abstract WmiObjectPath ManagementPath { get; }

    public abstract MetricEnabledState AggregateMetricEnabledState { get; }

    public event EventHandler Deleted;

    public event EventHandler CacheUpdated;

    public IVMDeviceSetting GetCapabilities(SettingsDefineCapabilities capability)
    {
        return null;
    }

    public abstract IVMTask BeginDelete();

    public abstract void EndDelete(IVMTask deleteTask);

    public abstract void Delete();

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

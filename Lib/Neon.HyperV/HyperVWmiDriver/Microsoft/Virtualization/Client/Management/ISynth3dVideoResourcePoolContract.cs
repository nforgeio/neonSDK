using System;
using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class ISynth3dVideoResourcePoolContract : ISynth3dVideoResourcePool, IResourcePool, IVirtualizationManagementObject, IDeleteableAsync, IDeleteable, IMetricMeasurableElement
{
	public bool Is3dVideoSupported => false;

	public bool IsGPUCapable => false;

	public abstract string PoolId { get; }

	public abstract VMDeviceSettingType DeviceSettingType { get; }

	public abstract bool Primordial { get; }

	public abstract IResourcePoolSetting Setting { get; }

	public abstract IEnumerable<IVMDeviceSetting> AllCapabilities { get; }

	public abstract IEnumerable<IVMDevice> PhysicalDevices { get; }

	public abstract IEnumerable<IResourcePool> ParentPools { get; }

	public abstract IEnumerable<IResourcePool> ChildPools { get; }

	public abstract IEnumerable<IResourcePoolAllocationSetting> AllocationSettings { get; }

	public abstract Server Server { get; }

	public abstract WmiObjectPath ManagementPath { get; }

	public abstract MetricEnabledState AggregateMetricEnabledState { get; }

	public event EventHandler Deleted;

	public event EventHandler CacheUpdated;

	public long CalculateVideoMemoryRequirements(int monitorResolution, int numberOfMonitors)
	{
		return 0L;
	}

	public abstract IVMDeviceSetting GetCapabilities(SettingsDefineCapabilities capability);

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

using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_ResourcePool")]
internal interface IResourcePool : IVirtualizationManagementObject, IDeleteableAsync, IDeleteable, IMetricMeasurableElement
{
	string PoolId { get; }

	VMDeviceSettingType DeviceSettingType { get; }

	bool Primordial { get; }

	IResourcePoolSetting Setting { get; }

	IEnumerable<IVMDeviceSetting> AllCapabilities { get; }

	IEnumerable<IVMDevice> PhysicalDevices { get; }

	IEnumerable<IResourcePool> ParentPools { get; }

	IEnumerable<IResourcePool> ChildPools { get; }

	IEnumerable<IResourcePoolAllocationSetting> AllocationSettings { get; }

	IVMDeviceSetting GetCapabilities(SettingsDefineCapabilities capability);
}

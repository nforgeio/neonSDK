using System;
using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_ComputerSystem", PrimaryMapping = false)]
internal interface IHostComputerSystem : IHostComputerSystemBase, IVirtualizationManagementObject
{
    IEnumerable<IResourcePool> ResourcePools { get; }

    IEnumerable<IExternalFcPort> ExternalFcPorts { get; }

    IEnumerable<IEthernetFeatureCapabilities> EthernetFeatureCapabilities { get; }

    IVMDeviceSetting GetSettingCapabilities(VMDeviceSettingType deviceType, SettingsDefineCapabilities capability);

    IVMDeviceSetting GetSettingCapabilities(VMDeviceSettingType deviceType, string poolId, SettingsDefineCapabilities capability);

    List<IResourcePool> GetResourcePools(VMDeviceSettingType deviceType);

    IResourcePool GetPrimordialResourcePool(VMDeviceSettingType deviceType);

    IEnumerable<IPhysicalProcessor> GetPhysicalProcessors();

    IEnumerable<IPhysicalCDRomDrive> GetPhysicalCDDrives(bool update, TimeSpan threshold);

    IEnumerable<IVMAssignableDevice> GetHostAssignableDevices(TimeSpan updateThreshold);

    IEnumerable<IEthernetFeature> GetAllFeatures(SettingsDefineCapabilities capability);

    IEthernetFeature GetFeatureCapabilities(EthernetFeatureType featureType, SettingsDefineCapabilities capability);
}

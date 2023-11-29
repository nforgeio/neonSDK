#define TRACE
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Microsoft.Virtualization.Client.Management;

internal class HostComputerSystemView : HostComputerSystemBaseView, IHostComputerSystem, IHostComputerSystemBase, IVirtualizationManagementObject
{
    private class PhysicalCdRomDriveComparer : IComparer<IPhysicalCDRomDrive>
    {
        public int Compare(IPhysicalCDRomDrive driveX, IPhysicalCDRomDrive driveY)
        {
            if (driveX != null && driveY != null)
            {
                return string.Compare(driveX.Drive, driveY.Drive, StringComparison.CurrentCultureIgnoreCase);
            }
            if (driveX != null)
            {
                return 1;
            }
            if (driveY != null)
            {
                return -1;
            }
            return 0;
        }
    }

    public IEnumerable<IResourcePool> ResourcePools => GetRelatedObjects<IResourcePool>(base.Associations.QueryResourcePools);

    public IEnumerable<IExternalFcPort> ExternalFcPorts => GetRelatedObjects<IExternalFcPort>(base.Associations.QueryExternalFcPorts);

    public IEnumerable<IEthernetFeatureCapabilities> EthernetFeatureCapabilities => GetRelatedObjects<IEthernetFeatureCapabilities>(base.Associations.QueryEthernetSwitchFeatureCapabilities);

    public List<IResourcePool> GetResourcePools(VMDeviceSettingType deviceType)
    {
        List<IResourcePool> list = ResourcePools.Where((IResourcePool p) => p.DeviceSettingType == deviceType).ToList();
        if (list.Count == 0)
        {
            VMTrace.TraceError(string.Format(CultureInfo.InvariantCulture, "Unable to find any resource pool for device type {0}", deviceType));
        }
        return list;
    }

    public IResourcePool GetPrimordialResourcePool(VMDeviceSettingType deviceType)
    {
        List<IResourcePool> resourcePools = GetResourcePools(deviceType);
        IResourcePool resourcePool = null;
        foreach (IResourcePool item in resourcePools)
        {
            if (item.Primordial)
            {
                resourcePool = item;
                break;
            }
        }
        if (resourcePool == null)
        {
            VMDeviceSettingTypeMapper.MapVMDeviceSettingTypeToResourceType(deviceType, out var resourceType, out var resourceSubType);
            VMTrace.TraceError(string.Format(CultureInfo.InvariantCulture, "Unable to find the primordial resource pool with ResourceType='{0}' AND ResourceSubtype='{1}'", resourceType, resourceSubType));
            throw ThrowHelper.CreateRelatedObjectNotFoundException(base.Server, typeof(IResourcePool));
        }
        return resourcePool;
    }

    public IVMDeviceSetting GetSettingCapabilities(VMDeviceSettingType deviceType, SettingsDefineCapabilities capability)
    {
        return GetPrimordialResourcePool(deviceType).GetCapabilities(capability);
    }

    public IVMDeviceSetting GetSettingCapabilities(VMDeviceSettingType deviceType, string poolId, SettingsDefineCapabilities capability)
    {
        IVMDeviceSetting iVMDeviceSetting = (from pool in ObjectLocator.GetResourcePoolsByNamesAndTypes(base.Server, new string[1] { poolId }, allowWildcards: false, new VMDeviceSettingType[1] { deviceType })
            select pool.GetCapabilities(capability)).FirstOrDefault();
        if (iVMDeviceSetting == null)
        {
            iVMDeviceSetting = GetPrimordialResourcePool(deviceType).GetCapabilities(capability);
        }
        return iVMDeviceSetting;
    }

    public IEnumerable<IPhysicalCDRomDrive> GetPhysicalCDDrives(bool update, TimeSpan threshold)
    {
        if (update)
        {
            base.Proxy.UpdateOneCachedAssociation(base.Associations.QueryWin32CDRomDrives, threshold);
        }
        List<IPhysicalCDRomDrive> list = new List<IPhysicalCDRomDrive>(GetRelatedObjects<IPhysicalCDRomDrive>(base.Associations.QueryWin32CDRomDrives));
        list.Sort(new PhysicalCdRomDriveComparer());
        return list;
    }

    public IEnumerable<IVMAssignableDevice> GetHostAssignableDevices(TimeSpan updateThreshold)
    {
        IResourcePool primordialResourcePool = GetPrimordialResourcePool(VMDeviceSettingType.PciExpress);
        primordialResourcePool.UpdateAssociationCache(updateThreshold);
        return primordialResourcePool.PhysicalDevices.Cast<IVMAssignableDevice>();
    }

    public IEnumerable<IPhysicalProcessor> GetPhysicalProcessors()
    {
        return GetRelatedObjects<IPhysicalProcessor>(base.Associations.QueryPhysicalProcessors);
    }

    public IEnumerable<IEthernetFeature> GetAllFeatures(SettingsDefineCapabilities capability)
    {
        return EthernetFeatureCapabilities.Select((IEthernetFeatureCapabilities featureCapability) => featureCapability.FeatureSettings.FirstOrDefault((IEthernetFeature fsdCapabilities) => capability.MatchByDescription(fsdCapabilities.InstanceId)));
    }

    public IEthernetFeature GetFeatureCapabilities(EthernetFeatureType featureType, SettingsDefineCapabilities capability)
    {
        VMTrace.TraceUserActionInitiated("Get feature setting capabilities information.");
        string featureId = EthernetFeatureTypeMapper.MapFeatureTypeToFeatureId(featureType);
        IEthernetFeature result = (EthernetFeatureCapabilities.FirstOrDefault((IEthernetFeatureCapabilities featureCapability) => featureCapability.FeatureId == featureId) ?? throw ThrowHelper.CreateRelatedObjectNotFoundException(base.Server, typeof(IEthernetFeatureCapabilities))).FeatureSettings.FirstOrDefault((IEthernetFeature fsdCapabilities) => capability.MatchByDescription(fsdCapabilities.InstanceId)) ?? throw ThrowHelper.CreateRelatedObjectNotFoundException(base.Server, typeof(IEthernetFeature));
        VMTrace.TraceUserActionCompleted("Retrieved switch feature setting data information successfully.");
        return result;
    }

    public override void UpdateAssociationCache(TimeSpan threshold)
    {
        base.Proxy.UpdateOneCachedAssociation(base.Associations.QueryTasks, threshold);
        base.Proxy.UpdateOneCachedAssociation(base.Associations.QueryResourcePools, threshold);
        base.Proxy.UpdateOneCachedAssociation(base.Associations.QueryExternalFcPorts, threshold);
    }
}

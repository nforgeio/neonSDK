using System;
using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IHostComputerSystemContract : IHostComputerSystem, IHostComputerSystemBase, IVirtualizationManagementObject
{
	public IEnumerable<IResourcePool> ResourcePools => null;

	public IEnumerable<IExternalFcPort> ExternalFcPorts => null;

	public IEnumerable<IEthernetFeatureCapabilities> EthernetFeatureCapabilities => null;

	public abstract IVMService VirtualizationService { get; }

	public abstract Server Server { get; }

	public abstract WmiObjectPath ManagementPath { get; }

	public event VMComputersSystemCreatedEventHandler VMComputerSystemCreated;

	public event VMVirtualizationTaskCreatedEventHandler VMVirtualizationTaskCreated;

	public event EventHandler Deleted;

	public event EventHandler CacheUpdated;

	public IVMDeviceSetting GetSettingCapabilities(VMDeviceSettingType deviceType, SettingsDefineCapabilities capability)
	{
		return null;
	}

	public IVMDeviceSetting GetSettingCapabilities(VMDeviceSettingType deviceType, string poolId, SettingsDefineCapabilities capability)
	{
		return null;
	}

	public List<IResourcePool> GetResourcePools(VMDeviceSettingType deviceType)
	{
		return null;
	}

	public IResourcePool GetPrimordialResourcePool(VMDeviceSettingType deviceType)
	{
		return null;
	}

	public IEnumerable<IPhysicalProcessor> GetPhysicalProcessors()
	{
		return null;
	}

	public IEnumerable<IPhysicalCDRomDrive> GetPhysicalCDDrives(bool update, TimeSpan threshold)
	{
		return null;
	}

	public IEnumerable<IVMAssignableDevice> GetHostAssignableDevices(TimeSpan threshold)
	{
		return null;
	}

	public IEnumerable<IEthernetFeature> GetAllFeatures(SettingsDefineCapabilities capability)
	{
		return null;
	}

	public IEthernetFeature GetFeatureCapabilities(EthernetFeatureType featureType, SettingsDefineCapabilities capability)
	{
		return null;
	}

	public abstract IList<ISummaryInformation> GetAllSummaryInformation(SummaryInformationRequest requestedInformation);

	public abstract IList<ISummaryInformation> GetSummaryInformation(IList<IVMComputerSystem> vmList, SummaryInformationRequest requestedInformation);

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

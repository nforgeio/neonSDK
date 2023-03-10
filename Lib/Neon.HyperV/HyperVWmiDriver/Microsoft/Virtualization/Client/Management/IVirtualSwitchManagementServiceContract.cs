using System;
using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IVirtualSwitchManagementServiceContract : IVirtualSwitchManagementService, IVirtualizationManagementObject, IEthernetSwitchFeatureService
{
	public IEnumerable<IVirtualEthernetSwitch> VirtualSwitches => null;

	public IEnumerable<IExternalNetworkPort> ExternalNetworkPorts => null;

	public IEnumerable<IInternalEthernetPort> InternalEthernetPorts => null;

	public IVirtualEthernetSwitchManagementCapabilities Capabilities => null;

	public abstract Server Server { get; }

	public abstract WmiObjectPath ManagementPath { get; }

	public event EventHandler Deleted;

	public event EventHandler CacheUpdated;

	public IVMTask BeginCreateVirtualSwitch(string friendlyName, string instanceId, string notes, bool iovPreferred, BandwidthReservationMode? bandwidthReservationMode, bool? packetDirectEnabled, bool? embeddedTeamingEnabled)
	{
		return null;
	}

	public IVirtualEthernetSwitch EndCreateVirtualSwitch(IVMTask task)
	{
		return null;
	}

	public IVMTask BeginDeleteVirtualSwitch(IVirtualEthernetSwitch virtualSwitch)
	{
		return null;
	}

	public void EndDeleteVirtualSwitch(IVMTask task)
	{
	}

	public IVMTask BeginAddVirtualSwitchPorts(IVirtualEthernetSwitch virtualSwitch, IEthernetPortAllocationSettingData[] portsToAdd)
	{
		return null;
	}

	public IEnumerable<IEthernetPortAllocationSettingData> EndAddVirtualSwitchPorts(IVMTask task)
	{
		return null;
	}

	public IVMTask BeginRemoveVirtualSwitchPorts(IVirtualEthernetSwitchPort[] portsToRemove)
	{
		return null;
	}

	public void EndRemoveVirtualSwitchPorts(IVMTask task)
	{
	}

	public IVMTask BeginModifyVirtualSwitchPorts(IVirtualEthernetSwitchPortSetting[] portsToModify)
	{
		return null;
	}

	public void EndModifyVirtualSwitchPorts(IVMTask task)
	{
	}

	public void UpdateSwitches(TimeSpan threshold)
	{
	}

	public void UpdateExternalNetworkPorts(TimeSpan threshold)
	{
	}

	public void UpdateInternalEthernetPorts(TimeSpan threshold)
	{
	}

	public abstract IVMTask BeginAddPortFeatures(IEthernetPortAllocationSettingData connectionRequest, IEthernetSwitchPortFeature[] features);

	public abstract IVMTask BeginAddPortFeatures(IEthernetPortAllocationSettingData connectionRequest, string[] featureEmbeddedInstances);

	public abstract IEnumerable<IEthernetSwitchPortFeature> EndAddPortFeatures(IVMTask task);

	public abstract IVMTask BeginAddSwitchFeatures(IVirtualEthernetSwitchSetting switchSetting, IEthernetSwitchFeature[] features);

	public abstract IVMTask BeginAddSwitchFeatures(IVirtualEthernetSwitchSetting switchSetting, string[] featureEmbeddedInstances);

	public abstract IEnumerable<IEthernetSwitchFeature> EndAddSwitchFeatures(IVMTask task);

	public abstract IVMTask BeginModifyFeatures(IEthernetFeature[] features);

	public abstract IVMTask BeginModifyFeatures(string[] featureEmbeddedInstances);

	public abstract void EndModifyFeatures(IVMTask task);

	public abstract IVMTask BeginRemoveFeatures(IEthernetFeature[] features);

	public abstract void EndRemoveFeatures(IVMTask task);

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

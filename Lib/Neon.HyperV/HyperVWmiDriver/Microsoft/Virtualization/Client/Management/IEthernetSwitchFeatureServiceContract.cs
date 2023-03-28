using System;
using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IEthernetSwitchFeatureServiceContract : IEthernetSwitchFeatureService, IVirtualizationManagementObject
{
	public abstract Server Server { get; }

	public abstract WmiObjectPath ManagementPath { get; }

	public event EventHandler Deleted;

	public event EventHandler CacheUpdated;

	public IVMTask BeginAddPortFeatures(IEthernetPortAllocationSettingData connectionRequest, IEthernetSwitchPortFeature[] features)
	{
		return null;
	}

	public IVMTask BeginAddPortFeatures(IEthernetPortAllocationSettingData connectionRequest, string[] featureEmbeddedInstances)
	{
		return null;
	}

	public IEnumerable<IEthernetSwitchPortFeature> EndAddPortFeatures(IVMTask task)
	{
		return null;
	}

	public IVMTask BeginAddSwitchFeatures(IVirtualEthernetSwitchSetting switchSetting, IEthernetSwitchFeature[] features)
	{
		return null;
	}

	public IVMTask BeginAddSwitchFeatures(IVirtualEthernetSwitchSetting switchSetting, string[] featureEmbeddedInstances)
	{
		return null;
	}

	public IEnumerable<IEthernetSwitchFeature> EndAddSwitchFeatures(IVMTask task)
	{
		return null;
	}

	public IVMTask BeginModifyFeatures(IEthernetFeature[] features)
	{
		return null;
	}

	public IVMTask BeginModifyFeatures(string[] featureEmbeddedInstances)
	{
		return null;
	}

	public void EndModifyFeatures(IVMTask task)
	{
	}

	public IVMTask BeginRemoveFeatures(IEthernetFeature[] features)
	{
		return null;
	}

	public void EndRemoveFeatures(IVMTask task)
	{
	}

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

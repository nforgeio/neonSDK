using System;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IEthernetSwitchPortIsolationFeatureContract : IEthernetSwitchPortIsolationFeature, IEthernetSwitchPortFeature, IEthernetFeature, IVirtualizationManagementObject
{
	public IsolationMode IsolationMode
	{
		get
		{
			return IsolationMode.Unknown;
		}
		set
		{
		}
	}

	public bool AllowUntaggedTraffic
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public int DefaultIsolationID
	{
		get
		{
			return 0;
		}
		set
		{
		}
	}

	public bool IsMultiTenantStackEnabled
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public abstract string InstanceId { get; }

	public abstract EthernetFeatureType FeatureType { get; }

	public abstract string Name { get; }

	public abstract string ExtensionId { get; }

	public abstract string FeatureId { get; }

	public abstract Server Server { get; }

	public abstract WmiObjectPath ManagementPath { get; }

	public event EventHandler Deleted;

	public event EventHandler CacheUpdated;

	public IVMTask BeginModifySingleFeature(IEthernetSwitchFeatureService service)
	{
		return null;
	}

	public void EndModifySingleFeature(IVMTask modifyTask)
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
[WmiName("Msvm_EthernetSwitchPortIsolationSettingData")]
internal interface IEthernetSwitchPortIsolationFeature : IEthernetSwitchPortFeature, IEthernetFeature, IVirtualizationManagementObject
{
	IsolationMode IsolationMode { get; set; }

	bool AllowUntaggedTraffic { get; set; }

	int DefaultIsolationID { get; set; }

	bool IsMultiTenantStackEnabled { get; set; }
}

using System;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IEthernetSwitchPortExtendedAclFeatureContract : IEthernetSwitchPortExtendedAclFeature, IEthernetSwitchPortFeature, IEthernetFeature, IVirtualizationManagementObject
{
	public AclDirection Direction
	{
		get
		{
			return AclDirection.Unknown;
		}
		set
		{
		}
	}

	public AclAction Action
	{
		get
		{
			return AclAction.Unknown;
		}
		set
		{
		}
	}

	public string LocalIPAddress
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public string RemoteIPAddress
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public string LocalPort
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public string RemotePort
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public string Protocol
	{
		get
		{
			return null;
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

	public bool IsStateful
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public int IdleSessionTimeout
	{
		get
		{
			return 0;
		}
		set
		{
		}
	}

	public int IsolationId
	{
		get
		{
			return 0;
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
[WmiName("Msvm_EthernetSwitchPortExtendedAclSettingData")]
internal interface IEthernetSwitchPortExtendedAclFeature : IEthernetSwitchPortFeature, IEthernetFeature, IVirtualizationManagementObject
{
	AclDirection Direction { get; set; }

	AclAction Action { get; set; }

	string LocalIPAddress { get; set; }

	string RemoteIPAddress { get; set; }

	string LocalPort { get; set; }

	string RemotePort { get; set; }

	string Protocol { get; set; }

	int Weight { get; set; }

	bool IsStateful { get; set; }

	int IdleSessionTimeout { get; set; }

	int IsolationId { get; set; }
}

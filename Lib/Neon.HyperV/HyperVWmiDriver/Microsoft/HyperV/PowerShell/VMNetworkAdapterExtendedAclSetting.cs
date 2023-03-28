using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMNetworkAdapterExtendedAclSetting : VMNetworkAdapterFeatureBase
{
	public VMNetworkAdapterExtendedAclDirection Direction
	{
		get
		{
			return (VMNetworkAdapterExtendedAclDirection)((IEthernetSwitchPortExtendedAclFeature)m_FeatureSetting).Direction;
		}
		internal set
		{
			((IEthernetSwitchPortExtendedAclFeature)m_FeatureSetting).Direction = (AclDirection)value;
		}
	}

	public VMNetworkAdapterExtendedAclAction Action
	{
		get
		{
			return (VMNetworkAdapterExtendedAclAction)((IEthernetSwitchPortExtendedAclFeature)m_FeatureSetting).Action;
		}
		internal set
		{
			((IEthernetSwitchPortExtendedAclFeature)m_FeatureSetting).Action = (AclAction)value;
		}
	}

	public string LocalIPAddress
	{
		get
		{
			return ((IEthernetSwitchPortExtendedAclFeature)m_FeatureSetting).LocalIPAddress;
		}
		internal set
		{
			((IEthernetSwitchPortExtendedAclFeature)m_FeatureSetting).LocalIPAddress = value;
		}
	}

	public string RemoteIPAddress
	{
		get
		{
			return ((IEthernetSwitchPortExtendedAclFeature)m_FeatureSetting).RemoteIPAddress;
		}
		internal set
		{
			((IEthernetSwitchPortExtendedAclFeature)m_FeatureSetting).RemoteIPAddress = value;
		}
	}

	public string LocalPort
	{
		get
		{
			return ((IEthernetSwitchPortExtendedAclFeature)m_FeatureSetting).LocalPort;
		}
		internal set
		{
			((IEthernetSwitchPortExtendedAclFeature)m_FeatureSetting).LocalPort = value;
		}
	}

	public string RemotePort
	{
		get
		{
			return ((IEthernetSwitchPortExtendedAclFeature)m_FeatureSetting).RemotePort;
		}
		internal set
		{
			((IEthernetSwitchPortExtendedAclFeature)m_FeatureSetting).RemotePort = value;
		}
	}

	public string Protocol
	{
		get
		{
			return ((IEthernetSwitchPortExtendedAclFeature)m_FeatureSetting).Protocol;
		}
		internal set
		{
			((IEthernetSwitchPortExtendedAclFeature)m_FeatureSetting).Protocol = value;
		}
	}

	public int Weight
	{
		get
		{
			return ((IEthernetSwitchPortExtendedAclFeature)m_FeatureSetting).Weight;
		}
		internal set
		{
			((IEthernetSwitchPortExtendedAclFeature)m_FeatureSetting).Weight = value;
		}
	}

	public bool Stateful
	{
		get
		{
			return ((IEthernetSwitchPortExtendedAclFeature)m_FeatureSetting).IsStateful;
		}
		internal set
		{
			((IEthernetSwitchPortExtendedAclFeature)m_FeatureSetting).IsStateful = value;
		}
	}

	public int IdleSessionTimeout
	{
		get
		{
			return ((IEthernetSwitchPortExtendedAclFeature)m_FeatureSetting).IdleSessionTimeout;
		}
		internal set
		{
			((IEthernetSwitchPortExtendedAclFeature)m_FeatureSetting).IdleSessionTimeout = value;
		}
	}

	public int IsolationID
	{
		get
		{
			return ((IEthernetSwitchPortExtendedAclFeature)m_FeatureSetting).IsolationId;
		}
		internal set
		{
			((IEthernetSwitchPortExtendedAclFeature)m_FeatureSetting).IsolationId = value;
		}
	}

	internal VMNetworkAdapterExtendedAclSetting(IEthernetSwitchPortExtendedAclFeature extendedAclSetting, VMNetworkAdapterBase parentAdapter)
		: base(extendedAclSetting, parentAdapter, isTemplate: false)
	{
	}

	private VMNetworkAdapterExtendedAclSetting(VMNetworkAdapterBase parentAdapter)
		: base(parentAdapter, EthernetFeatureType.ExtendedAcl)
	{
	}

	internal static VMNetworkAdapterExtendedAclSetting CreateTemplateExtendedAclSetting(VMNetworkAdapterBase parentAdapter)
	{
		return new VMNetworkAdapterExtendedAclSetting(parentAdapter);
	}
}

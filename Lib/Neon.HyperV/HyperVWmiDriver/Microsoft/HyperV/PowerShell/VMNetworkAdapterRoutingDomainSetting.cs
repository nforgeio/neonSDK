using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMNetworkAdapterRoutingDomainSetting : VMNetworkAdapterFeatureBase
{
	public Guid RoutingDomainID
	{
		get
		{
			return ((IEthernetSwitchPortRoutingDomainFeature)m_FeatureSetting).RoutingDomainId;
		}
		internal set
		{
			((IEthernetSwitchPortRoutingDomainFeature)m_FeatureSetting).RoutingDomainId = value;
		}
	}

	public string RoutingDomainName
	{
		get
		{
			return ((IEthernetSwitchPortRoutingDomainFeature)m_FeatureSetting).RoutingDomainName;
		}
		internal set
		{
			((IEthernetSwitchPortRoutingDomainFeature)m_FeatureSetting).RoutingDomainName = value;
		}
	}

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
	public int[] IsolationID
	{
		get
		{
			return ((IEthernetSwitchPortRoutingDomainFeature)m_FeatureSetting).IsolationIds.ToArray();
		}
		internal set
		{
			((IEthernetSwitchPortRoutingDomainFeature)m_FeatureSetting).IsolationIds = (IReadOnlyCollection<int>)(object)value;
		}
	}

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
	public string[] IsolationName
	{
		get
		{
			return ((IEthernetSwitchPortRoutingDomainFeature)m_FeatureSetting).IsolationNames.ToArray();
		}
		internal set
		{
			((IEthernetSwitchPortRoutingDomainFeature)m_FeatureSetting).IsolationNames = (IReadOnlyCollection<string>)(object)value;
		}
	}

	internal VMNetworkAdapterRoutingDomainSetting(IEthernetSwitchPortRoutingDomainFeature routingDomainSetting, VMNetworkAdapterBase parentAdapter)
		: base(routingDomainSetting, parentAdapter, isTemplate: false)
	{
	}

	private VMNetworkAdapterRoutingDomainSetting(VMNetworkAdapterBase parentAdapter)
		: base(parentAdapter, EthernetFeatureType.RoutingDomain)
	{
	}

	internal static VMNetworkAdapterRoutingDomainSetting CreateTemplateRoutingDomainSetting(VMNetworkAdapterBase parentAdapter)
	{
		return new VMNetworkAdapterRoutingDomainSetting(parentAdapter);
	}
}

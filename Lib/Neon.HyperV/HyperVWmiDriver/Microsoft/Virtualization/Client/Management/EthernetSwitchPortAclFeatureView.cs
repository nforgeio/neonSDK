using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Virtualization.Client.Management;

internal sealed class EthernetSwitchPortAclFeatureView : EthernetSwitchPortFeatureView, IEthernetSwitchPortAclFeature, IEthernetSwitchPortFeature, IEthernetFeature, IVirtualizationManagementObject, IMetricMeasurableElement
{
	internal static class WmiMemberNames
	{
		public const string Direction = "Direction";

		public const string Action = "Action";

		public const string AddressType = "AclType";

		public const string Applicability = "Applicability";

		public const string LocalAddress = "LocalAddress";

		public const string LocalAddressPrefixLength = "LocalAddressPrefixLength";

		public const string RemoteAddress = "RemoteAddress";

		public const string RemoteAddressPrefixLength = "RemoteAddressPrefixLength";
	}

	public override EthernetFeatureType FeatureType => EthernetFeatureType.Acl;

	public AclAddressType AddressType
	{
		get
		{
			return (AclAddressType)GetProperty<byte>("AclType");
		}
		set
		{
			SetProperty("AclType", (byte)value);
		}
	}

	public AclDirection Direction
	{
		get
		{
			return (AclDirection)GetProperty<byte>("Direction");
		}
		set
		{
			SetProperty("Direction", (byte)value);
		}
	}

	public AclAction Action
	{
		get
		{
			return (AclAction)GetProperty<byte>("Action");
		}
		set
		{
			SetProperty("Action", (byte)value);
		}
	}

	public string LocalAddress
	{
		get
		{
			return GetProperty<string>("LocalAddress");
		}
		set
		{
			SetProperty("LocalAddress", value);
		}
	}

	public byte LocalAddressPrefixLength
	{
		get
		{
			return GetProperty<byte>("LocalAddressPrefixLength");
		}
		set
		{
			SetProperty("LocalAddressPrefixLength", value);
		}
	}

	public string RemoteAddress
	{
		get
		{
			return GetProperty<string>("RemoteAddress");
		}
		set
		{
			SetProperty("RemoteAddress", value);
		}
	}

	public byte RemoteAddressPrefixLength
	{
		get
		{
			return GetProperty<byte>("RemoteAddressPrefixLength");
		}
		set
		{
			SetProperty("RemoteAddressPrefixLength", value);
		}
	}

	public bool IsRemote
	{
		get
		{
			return GetProperty<byte>("Applicability") == 2;
		}
		set
		{
			AclApplicability aclApplicability = ((!value) ? AclApplicability.Local : AclApplicability.Remote);
			SetProperty("Applicability", aclApplicability);
		}
	}

	public MetricEnabledState AggregateMetricEnabledState => MetricServiceView.CalculateAggregatedMetricEnabledState(GetRelatedObjects<IMeasuredElementToMetricDefinitionAssociation>(base.Associations.MeasuredElementToMetricDefRelationship));

	public IReadOnlyCollection<IMetricValue> GetMetricValues()
	{
		return GetRelatedObjects<IMetricValue>(base.Associations.MeasuredElementToMetricValue).ToList();
	}
}

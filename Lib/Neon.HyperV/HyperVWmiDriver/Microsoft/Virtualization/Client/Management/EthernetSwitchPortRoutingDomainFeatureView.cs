using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Microsoft.Virtualization.Client.Management;

internal sealed class EthernetSwitchPortRoutingDomainFeatureView : EthernetSwitchPortFeatureView, IEthernetSwitchPortRoutingDomainFeature, IEthernetSwitchPortFeature, IEthernetFeature, IVirtualizationManagementObject
{
    internal static class WmiMemberNames
    {
        public const string RoutingDomainId = "RoutingDomainGuid";

        public const string RoutingDomainName = "RoutingDomainName";

        public const string IsolationIds = "IsolationIdList";

        public const string IsolationNames = "IsolationIdNameList";
    }

    public override EthernetFeatureType FeatureType => EthernetFeatureType.RoutingDomain;

    public IReadOnlyCollection<int> IsolationIds
    {
        get
        {
            return GetProperty<uint[]>("IsolationIdList").Select(NumberConverter.UInt32ToInt32).ToList();
        }
        set
        {
            uint[] value2 = value.Select(NumberConverter.Int32ToUInt32).ToArray();
            SetProperty("IsolationIdList", value2);
        }
    }

    public IReadOnlyCollection<string> IsolationNames
    {
        get
        {
            return new ReadOnlyCollection<string>(GetProperty<string[]>("IsolationIdNameList"));
        }
        set
        {
            SetProperty("IsolationIdNameList", value.ToArray());
        }
    }

    public Guid RoutingDomainId
    {
        get
        {
            string property = GetProperty<string>("RoutingDomainGuid");
            if (property == null)
            {
                return Guid.Empty;
            }
            return Guid.Parse(property);
        }
        set
        {
            SetProperty("RoutingDomainGuid", value.ToString("B"));
        }
    }

    public string RoutingDomainName
    {
        get
        {
            return GetProperty<string>("RoutingDomainName");
        }
        set
        {
            SetProperty("RoutingDomainName", value);
        }
    }
}

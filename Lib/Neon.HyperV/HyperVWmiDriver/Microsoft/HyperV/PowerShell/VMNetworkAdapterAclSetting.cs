#define TRACE
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Virtualization.Client;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMNetworkAdapterAclSetting : VMNetworkAdapterFeatureBase
{
    internal const string WildcardIPv4 = "0.0.0.0/0";

    internal const string WildcardIPv6 = "::/0";

    internal const string WildcardBoth = "ANY";

    internal const string WildcardMac = "ANY";

    public VMNetworkAdapterAclAction Action
    {
        get
        {
            return (VMNetworkAdapterAclAction)((IEthernetSwitchPortAclFeature)m_FeatureSetting).Action;
        }
        internal set
        {
            ((IEthernetSwitchPortAclFeature)m_FeatureSetting).Action = (AclAction)value;
        }
    }

    public VMNetworkAdapterAclDirection Direction
    {
        get
        {
            return (VMNetworkAdapterAclDirection)((IEthernetSwitchPortAclFeature)m_FeatureSetting).Direction;
        }
        internal set
        {
            ((IEthernetSwitchPortAclFeature)m_FeatureSetting).Direction = (AclDirection)value;
        }
    }

    public string LocalAddress
    {
        get
        {
            IEthernetSwitchPortAclFeature ethernetSwitchPortAclFeature = (IEthernetSwitchPortAclFeature)m_FeatureSetting;
            if (!ethernetSwitchPortAclFeature.IsRemote)
            {
                return FormatAddress(ethernetSwitchPortAclFeature.LocalAddress, ethernetSwitchPortAclFeature.LocalAddressPrefixLength, ethernetSwitchPortAclFeature.AddressType);
            }
            return null;
        }
    }

    public VMNetworkAdapterAclType LocalAddressType
    {
        get
        {
            IEthernetSwitchPortAclFeature ethernetSwitchPortAclFeature = (IEthernetSwitchPortAclFeature)m_FeatureSetting;
            if (!ethernetSwitchPortAclFeature.IsRemote)
            {
                return ComputeAclType(LocalAddress, ethernetSwitchPortAclFeature.AddressType);
            }
            return (VMNetworkAdapterAclType)0;
        }
    }

    public string MeteredMegabytes
    {
        get
        {
            IEthernetSwitchPortAclFeature obj = (IEthernetSwitchPortAclFeature)m_FeatureSetting;
            obj.UpdateAssociationCache(Constants.UpdateThreshold);
            return obj.GetMetricValues().FirstOrDefault()?.RawValue;
        }
    }

    public string RemoteAddress
    {
        get
        {
            IEthernetSwitchPortAclFeature ethernetSwitchPortAclFeature = (IEthernetSwitchPortAclFeature)m_FeatureSetting;
            if (ethernetSwitchPortAclFeature.IsRemote)
            {
                return FormatAddress(ethernetSwitchPortAclFeature.RemoteAddress, ethernetSwitchPortAclFeature.RemoteAddressPrefixLength, ethernetSwitchPortAclFeature.AddressType);
            }
            return null;
        }
    }

    public VMNetworkAdapterAclType RemoteAddressType
    {
        get
        {
            IEthernetSwitchPortAclFeature ethernetSwitchPortAclFeature = (IEthernetSwitchPortAclFeature)m_FeatureSetting;
            if (ethernetSwitchPortAclFeature.IsRemote)
            {
                return ComputeAclType(RemoteAddress, ethernetSwitchPortAclFeature.AddressType);
            }
            return (VMNetworkAdapterAclType)0;
        }
    }

    internal bool IsMacAddress => ((IEthernetSwitchPortAclFeature)m_FeatureSetting).AddressType == AclAddressType.Mac;

    internal VMNetworkAdapterAclSetting(IEthernetSwitchPortAclFeature aclSetting, VMNetworkAdapterBase parentAdapter)
        : base(aclSetting, parentAdapter, isTemplate: false)
    {
    }

    private VMNetworkAdapterAclSetting(VMNetworkAdapterBase parentAdapter)
        : base(parentAdapter, EthernetFeatureType.Acl)
    {
    }

    internal static VMNetworkAdapterAclSetting CreateTemplateAclSetting(VMNetworkAdapterBase parentAdapter)
    {
        return new VMNetworkAdapterAclSetting(parentAdapter);
    }

    internal IEnumerable<IMetricValue> GetMetricValues()
    {
        IEthernetSwitchPortAclFeature obj = (IEthernetSwitchPortAclFeature)m_FeatureSetting;
        obj.UpdateAssociationCache(Constants.UpdateThreshold);
        return obj.GetMetricValues();
    }

    internal void SetAddress(string inputAddress, bool isRemote, bool isMacAddress)
    {
        string address;
        byte prefixLength;
        if (IsWildcardAddress(inputAddress, isMacAddress, out var addressType))
        {
            address = string.Empty;
            prefixLength = 0;
        }
        else
        {
            ParseAddress(inputAddress, isMacAddress, out address, out prefixLength, out addressType);
        }
        IEthernetSwitchPortAclFeature ethernetSwitchPortAclFeature = (IEthernetSwitchPortAclFeature)m_FeatureSetting;
        if (isRemote)
        {
            ethernetSwitchPortAclFeature.RemoteAddress = address;
            ethernetSwitchPortAclFeature.RemoteAddressPrefixLength = prefixLength;
        }
        else
        {
            ethernetSwitchPortAclFeature.LocalAddress = address;
            ethernetSwitchPortAclFeature.LocalAddressPrefixLength = prefixLength;
        }
        ethernetSwitchPortAclFeature.AddressType = addressType;
        ethernetSwitchPortAclFeature.IsRemote = isRemote;
    }

    private static bool IsWildcardAddress(string inputAddress, bool isMacAddress, out AclAddressType addressType)
    {
        bool result = false;
        if (isMacAddress)
        {
            addressType = AclAddressType.Mac;
            if (string.Equals(inputAddress, "ANY", StringComparison.OrdinalIgnoreCase))
            {
                result = true;
            }
        }
        else if (string.Equals(inputAddress, "0.0.0.0/0", StringComparison.OrdinalIgnoreCase))
        {
            result = true;
            addressType = AclAddressType.Ipv4;
        }
        else if (string.Equals(inputAddress, "::/0", StringComparison.OrdinalIgnoreCase))
        {
            result = true;
            addressType = AclAddressType.Ipv6;
        }
        else
        {
            addressType = AclAddressType.Unknown;
        }
        return result;
    }

    private static void ParseAddress(string inputAddress, bool isMacAddress, out string address, out byte prefixLength, out AclAddressType addressType)
    {
        string[] array = inputAddress.Split('/');
        address = array[0];
        if (isMacAddress)
        {
            addressType = AclAddressType.Mac;
        }
        else if (address.Split('.').Length == 4)
        {
            addressType = AclAddressType.Ipv4;
        }
        else
        {
            addressType = AclAddressType.Ipv6;
        }
        if (array.Length > 1)
        {
            prefixLength = byte.Parse(array[1], CultureInfo.InvariantCulture);
        }
        else
        {
            prefixLength = GetMaximumPrefixLength(addressType);
        }
    }

    private static byte GetMaximumPrefixLength(AclAddressType addressType)
    {
        return addressType switch
        {
            AclAddressType.Ipv4 => 32, 
            AclAddressType.Ipv6 => 128, 
            AclAddressType.Mac => 48, 
            _ => 0, 
        };
    }

    private static string FormatAddress(string address, byte prefixLength, AclAddressType addressType)
    {
        if (string.IsNullOrEmpty(address))
        {
            return addressType switch
            {
                AclAddressType.Ipv4 => "0.0.0.0/0", 
                AclAddressType.Ipv6 => "::/0", 
                AclAddressType.Mac => "ANY", 
                _ => null, 
            };
        }
        if (prefixLength == GetMaximumPrefixLength(addressType))
        {
            return address;
        }
        return string.Format(CultureInfo.InvariantCulture, "{0}/{1}", address, prefixLength);
    }

    private static VMNetworkAdapterAclType ComputeAclType(string address, AclAddressType addressType)
    {
        switch (addressType)
        {
        case AclAddressType.Mac:
            return (!string.Equals(address, "ANY", StringComparison.OrdinalIgnoreCase)) ? VMNetworkAdapterAclType.Mac : VMNetworkAdapterAclType.WildcardMac;
        case AclAddressType.Ipv4:
            return string.Equals(address, "0.0.0.0/0", StringComparison.OrdinalIgnoreCase) ? VMNetworkAdapterAclType.WildcardIPv4 : VMNetworkAdapterAclType.IPv4;
        case AclAddressType.Ipv6:
            return string.Equals(address, "::/0", StringComparison.OrdinalIgnoreCase) ? VMNetworkAdapterAclType.WildcardIPv6 : VMNetworkAdapterAclType.IPv6;
        default:
            VMTrace.TraceWarning(string.Format(CultureInfo.InvariantCulture, "Unexpected ACL address type: {0}", addressType));
            return (VMNetworkAdapterAclType)0;
        }
    }
}

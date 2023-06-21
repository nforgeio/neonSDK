using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Microsoft.HyperV.PowerShell.Common;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMNetworkAdapterFailoverSetting : IUpdatable
{
    private readonly IFailoverNetworkAdapterSetting m_NetworkAdapterSetting;

    public string IPv4Address { get; internal set; }

    public string IPv4SubnetMask { get; internal set; }

    public string IPv4DefaultGateway { get; internal set; }

    public string IPv4PreferredDNSServer { get; internal set; }

    public string IPv4AlternateDNSServer { get; internal set; }

    public string IPv6Address { get; internal set; }

    public string IPv6SubnetPrefixLength { get; internal set; }

    public string IPv6DefaultGateway { get; internal set; }

    public string IPv6PreferredDNSServer { get; internal set; }

    public string IPv6AlternateDNSServer { get; internal set; }

    public WmiProtocolIFType ProtocolIFType { get; private set; }

    internal VMNetworkAdapterFailoverSetting(IFailoverNetworkAdapterSetting setting)
    {
        m_NetworkAdapterSetting = setting;
        InitProperties();
    }

    void IUpdatable.Put(IOperationWatcher watcher)
    {
        SetPropertiesInWmiObject();
        watcher.PerformPut(m_NetworkAdapterSetting, TaskDescriptions.Task_UpdateNetworkAdapterFailoverConfiguration, null);
    }

    internal static bool IsValidIP(string ip, AddressFamily addressFamily)
    {
        if (!string.IsNullOrWhiteSpace(ip) && IPAddress.TryParse(ip, out var address))
        {
            return address.AddressFamily == addressFamily;
        }
        return false;
    }

    internal static bool IsValidSubnetMask(string subnetMask, AddressFamily addressFamily)
    {
        if (string.IsNullOrWhiteSpace(subnetMask))
        {
            return false;
        }
        IPAddress address;
        if (addressFamily == AddressFamily.InterNetworkV6)
        {
            if (int.TryParse(subnetMask, out var result) && result >= 0 && result <= 128)
            {
                return true;
            }
        }
        else if (IPAddress.TryParse(subnetMask, out address) && address.AddressFamily == addressFamily)
        {
            byte[] addressBytes = address.GetAddressBytes();
            int num = 0;
            for (int i = 0; i < addressBytes.Length; i++)
            {
                num = (num << 8) | addressBytes[i];
            }
            for (int j = 0; j < 32; j++)
            {
                if ((num & 0x80000000u) == 0L)
                {
                    break;
                }
                num <<= 1;
            }
            return num == 0;
        }
        return false;
    }

    private static void SetValueToNonNullEntries(Action<string[]> setValue, params string[] args)
    {
        if (args != null)
        {
            List<string> list = new List<string>(args.Length);
            list.AddRange(args.Where((string arg) => !string.IsNullOrEmpty(arg)));
            setValue(list.ToArray());
        }
        else
        {
            setValue(new string[0]);
        }
    }

    private void InitProperties()
    {
        ProtocolIFType = (WmiProtocolIFType)m_NetworkAdapterSetting.ProtocolIFType;
        InitIpAddresses();
        InitDnsServers();
        InitDefaultGateways();
    }

    private void InitIpAddresses()
    {
        string[] iPAddresses = m_NetworkAdapterSetting.IPAddresses;
        string[] subnets = m_NetworkAdapterSetting.Subnets;
        switch (ProtocolIFType)
        {
        case WmiProtocolIFType.IPv4:
            IPv4Address = iPAddresses.FirstOrDefault((string p) => IsValidIP(p, AddressFamily.InterNetwork));
            IPv4SubnetMask = subnets.FirstOrDefault((string p) => IsValidSubnetMask(p, AddressFamily.InterNetwork));
            break;
        case WmiProtocolIFType.IPv6:
            IPv6Address = iPAddresses.FirstOrDefault((string p) => IsValidIP(p, AddressFamily.InterNetworkV6));
            IPv6SubnetPrefixLength = subnets.FirstOrDefault((string p) => IsValidSubnetMask(p, AddressFamily.InterNetworkV6));
            break;
        case WmiProtocolIFType.IPv4v6:
            IPv4Address = iPAddresses.FirstOrDefault((string p) => IsValidIP(p, AddressFamily.InterNetwork));
            IPv4SubnetMask = subnets.FirstOrDefault((string p) => IsValidSubnetMask(p, AddressFamily.InterNetwork));
            IPv6Address = iPAddresses.FirstOrDefault((string p) => IsValidIP(p, AddressFamily.InterNetworkV6));
            IPv6SubnetPrefixLength = subnets.FirstOrDefault((string p) => IsValidSubnetMask(p, AddressFamily.InterNetworkV6));
            break;
        }
    }

    private void InitDnsServers()
    {
        string[] dnsServers = m_NetworkAdapterSetting.DnsServers;
        List<string> list = new List<string>(3);
        List<string> list2 = new List<string>(3);
        string[] array = dnsServers;
        foreach (string text in array)
        {
            if (IsValidIP(text, AddressFamily.InterNetwork))
            {
                list.Add(text);
            }
            else if (IsValidIP(text, AddressFamily.InterNetworkV6))
            {
                list2.Add(text);
            }
        }
        switch (ProtocolIFType)
        {
        case WmiProtocolIFType.IPv4:
        {
            int count4 = list.Count;
            if (count4 > 0)
            {
                IPv4PreferredDNSServer = list[0];
            }
            if (count4 > 1)
            {
                IPv4AlternateDNSServer = list[1];
            }
            break;
        }
        case WmiProtocolIFType.IPv6:
        {
            int count3 = list2.Count;
            if (count3 > 0)
            {
                IPv6PreferredDNSServer = list2[0];
            }
            if (count3 > 1)
            {
                IPv6AlternateDNSServer = list2[1];
            }
            break;
        }
        case WmiProtocolIFType.IPv4v6:
        {
            int count = list.Count;
            if (count > 0)
            {
                IPv4PreferredDNSServer = list[0];
            }
            if (count > 1)
            {
                IPv4AlternateDNSServer = list[1];
            }
            int count2 = list2.Count;
            if (count2 > 0)
            {
                IPv6PreferredDNSServer = list2[0];
            }
            if (count2 > 1)
            {
                IPv6AlternateDNSServer = list2[1];
            }
            break;
        }
        }
    }

    private void InitDefaultGateways()
    {
        string[] defaultGateways = m_NetworkAdapterSetting.DefaultGateways;
        switch (ProtocolIFType)
        {
        case WmiProtocolIFType.IPv4:
            IPv4DefaultGateway = defaultGateways.FirstOrDefault((string p) => IsValidIP(p, AddressFamily.InterNetwork));
            break;
        case WmiProtocolIFType.IPv6:
            IPv6DefaultGateway = defaultGateways.FirstOrDefault((string p) => IsValidIP(p, AddressFamily.InterNetworkV6));
            break;
        case WmiProtocolIFType.IPv4v6:
            IPv4DefaultGateway = defaultGateways.FirstOrDefault((string p) => IsValidIP(p, AddressFamily.InterNetwork));
            IPv6DefaultGateway = defaultGateways.FirstOrDefault((string p) => IsValidIP(p, AddressFamily.InterNetworkV6));
            break;
        }
    }

    private void SetPropertiesInWmiObject()
    {
        if (!string.IsNullOrEmpty(IPv4Address) && !string.IsNullOrEmpty(IPv6Address))
        {
            m_NetworkAdapterSetting.IPAddresses = new string[2] { IPv4Address, IPv6Address };
            m_NetworkAdapterSetting.Subnets = new string[2] { IPv4SubnetMask, IPv6SubnetPrefixLength };
            SetValueToNonNullEntries(delegate(string[] value)
            {
                m_NetworkAdapterSetting.DefaultGateways = value;
            }, IPv4DefaultGateway, IPv6DefaultGateway);
            SetValueToNonNullEntries(delegate(string[] value)
            {
                m_NetworkAdapterSetting.DnsServers = value;
            }, IPv4PreferredDNSServer, IPv4AlternateDNSServer, IPv6PreferredDNSServer, IPv6AlternateDNSServer);
            m_NetworkAdapterSetting.DhcpEnabled = false;
            m_NetworkAdapterSetting.ProtocolIFType = VMNetworkAdapterProtocolType.IPv4v6;
        }
        else if (!string.IsNullOrEmpty(IPv4Address))
        {
            m_NetworkAdapterSetting.IPAddresses = new string[1] { IPv4Address };
            m_NetworkAdapterSetting.Subnets = new string[1] { IPv4SubnetMask };
            SetValueToNonNullEntries(delegate(string[] value)
            {
                m_NetworkAdapterSetting.DefaultGateways = value;
            }, IPv4DefaultGateway);
            SetValueToNonNullEntries(delegate(string[] value)
            {
                m_NetworkAdapterSetting.DnsServers = value;
            }, IPv4PreferredDNSServer, IPv4AlternateDNSServer);
            m_NetworkAdapterSetting.DhcpEnabled = false;
            m_NetworkAdapterSetting.ProtocolIFType = VMNetworkAdapterProtocolType.IPv4;
        }
        else if (!string.IsNullOrEmpty(IPv6Address))
        {
            m_NetworkAdapterSetting.IPAddresses = new string[1] { IPv6Address };
            m_NetworkAdapterSetting.Subnets = new string[1] { IPv6SubnetPrefixLength };
            SetValueToNonNullEntries(delegate(string[] value)
            {
                m_NetworkAdapterSetting.DefaultGateways = value;
            }, IPv6DefaultGateway);
            SetValueToNonNullEntries(delegate(string[] value)
            {
                m_NetworkAdapterSetting.DnsServers = value;
            }, IPv6PreferredDNSServer, IPv6AlternateDNSServer);
            m_NetworkAdapterSetting.DhcpEnabled = true;
            m_NetworkAdapterSetting.ProtocolIFType = VMNetworkAdapterProtocolType.IPv6;
        }
        else
        {
            m_NetworkAdapterSetting.IPAddresses = new string[0];
            m_NetworkAdapterSetting.Subnets = new string[0];
            m_NetworkAdapterSetting.DefaultGateways = new string[0];
            m_NetworkAdapterSetting.DnsServers = new string[0];
            m_NetworkAdapterSetting.DhcpEnabled = true;
            m_NetworkAdapterSetting.ProtocolIFType = VMNetworkAdapterProtocolType.Unknown;
        }
    }
}

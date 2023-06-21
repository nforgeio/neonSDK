#define TRACE
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Virtualization.Client.Management;

internal class FailoverNetworkAdapterSettingView : View, IFailoverNetworkAdapterSetting, IVirtualizationManagementObject, IPutableAsync, IPutable
{
    internal static class WmiMemberNames
    {
        public const string ProtocolIFType = "ProtocolIFType";

        public const string DhcpEnabled = "DHCPEnabled";

        public const string IPAddresses = "IPAddresses";

        public const string Subnets = "Subnets";

        public const string DefaultGateways = "DefaultGateways";

        public const string DnsServers = "DNSServers";

        public const string Put = "SetFailoverNetworkAdapterSettings";
    }

    public VMNetworkAdapterProtocolType ProtocolIFType
    {
        get
        {
            return (VMNetworkAdapterProtocolType)GetProperty<ushort>("ProtocolIFType");
        }
        set
        {
            SetProperty("ProtocolIFType", (ushort)value);
        }
    }

    public bool DhcpEnabled
    {
        get
        {
            return GetProperty<bool>("DHCPEnabled");
        }
        set
        {
            SetProperty("DHCPEnabled", value);
        }
    }

    public string[] IPAddresses
    {
        get
        {
            return GetProperty<string[]>("IPAddresses");
        }
        set
        {
            SetProperty("IPAddresses", value);
        }
    }

    public string[] Subnets
    {
        get
        {
            return GetProperty<string[]>("Subnets");
        }
        set
        {
            SetProperty("Subnets", value);
        }
    }

    public string[] DefaultGateways
    {
        get
        {
            return GetProperty<string[]>("DefaultGateways");
        }
        set
        {
            SetProperty("DefaultGateways", value);
        }
    }

    public string[] DnsServers
    {
        get
        {
            return GetProperty<string[]>("DNSServers");
        }
        set
        {
            SetProperty("DNSServers", value);
        }
    }

    protected override IVMTask BeginPutInternal(IDictionary<string, object> properties)
    {
        string[] array = new string[1] { GetEmbeddedInstance(properties) };
        IProxy failoverReplicationServiceProxy = GetFailoverReplicationServiceProxy();
        IVMDeviceSetting relatedObject = GetRelatedObject<IVMDeviceSetting>(base.Associations.EthernetPortToFailoverNetwork);
        IVMComputerSystemBase vMComputerSystem = relatedObject.VirtualComputerSystemSetting.VMComputerSystem;
        object[] array2 = new object[3] { vMComputerSystem, array, null };
        VMTrace.TraceUserActionInitiatedWithProperties(string.Format(CultureInfo.InvariantCulture, "Starting modify failover network ip settings for '{0}' ('{1}')", vMComputerSystem.InstanceId, relatedObject.FriendlyName), properties);
        uint result = failoverReplicationServiceProxy.InvokeMethod("SetFailoverNetworkAdapterSettings", array2);
        IVMTask iVMTask = BeginMethodTaskReturn(result, null, array2[2]);
        iVMTask.PutProperties = properties;
        return iVMTask;
    }
}

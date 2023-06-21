namespace Microsoft.Virtualization.Client.Management;

internal sealed class EthernetSwitchPortExtendedAclFeatureView : EthernetSwitchPortFeatureView, IEthernetSwitchPortExtendedAclFeature, IEthernetSwitchPortFeature, IEthernetFeature, IVirtualizationManagementObject
{
    internal static class WmiMemberNames
    {
        public const string Direction = "Direction";

        public const string Action = "Action";

        public const string LocalIPAddress = "LocalIPAddress";

        public const string RemoteIPAddress = "RemoteIPAddress";

        public const string LocalPort = "LocalPort";

        public const string RemotePort = "RemotePort";

        public const string Protocol = "Protocol";

        public const string Weight = "Weight";

        public const string IsStateful = "Stateful";

        public const string IdleSessionTimeout = "IdleSessionTimeout";

        public const string IsolationId = "IsolationId";
    }

    public override EthernetFeatureType FeatureType => EthernetFeatureType.ExtendedAcl;

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

    public string LocalIPAddress
    {
        get
        {
            return GetProperty<string>("LocalIPAddress");
        }
        set
        {
            SetProperty("LocalIPAddress", value);
        }
    }

    public string RemoteIPAddress
    {
        get
        {
            return GetProperty<string>("RemoteIPAddress");
        }
        set
        {
            SetProperty("RemoteIPAddress", value);
        }
    }

    public string LocalPort
    {
        get
        {
            return GetProperty<string>("LocalPort");
        }
        set
        {
            SetProperty("LocalPort", value);
        }
    }

    public string RemotePort
    {
        get
        {
            return GetProperty<string>("RemotePort");
        }
        set
        {
            SetProperty("RemotePort", value);
        }
    }

    public string Protocol
    {
        get
        {
            return GetProperty<string>("Protocol");
        }
        set
        {
            SetProperty("Protocol", value);
        }
    }

    public int Weight
    {
        get
        {
            return NumberConverter.UInt16ToInt32(GetProperty<ushort>("Weight"));
        }
        set
        {
            ushort num = NumberConverter.Int32ToUInt16(value);
            SetProperty("Weight", num);
        }
    }

    public bool IsStateful
    {
        get
        {
            return GetProperty<bool>("Stateful");
        }
        set
        {
            SetProperty("Stateful", value);
        }
    }

    public int IdleSessionTimeout
    {
        get
        {
            return NumberConverter.UInt16ToInt32(GetProperty<ushort>("IdleSessionTimeout"));
        }
        set
        {
            ushort num = NumberConverter.Int32ToUInt16(value);
            SetProperty("IdleSessionTimeout", num);
        }
    }

    public int IsolationId
    {
        get
        {
            return NumberConverter.UInt32ToInt32(GetProperty<uint>("IsolationId"));
        }
        set
        {
            uint num = NumberConverter.Int32ToUInt32(value);
            SetProperty("IsolationId", num);
        }
    }
}

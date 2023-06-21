namespace Microsoft.Virtualization.Client.Management;

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

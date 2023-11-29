namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_LANEndpoint")]
internal interface ILanEndpoint : IVirtualizationManagementObject
{
    IEthernetPort EthernetPort { get; }

    ILanEndpoint OtherEndpoint { get; }

    VMLanEndpointOperationalStatus[] OperationalStatus { get; }

    string[] StatusDescriptions { get; }
}

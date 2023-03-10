namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_ExternalEthernetPort")]
internal interface IExternalEthernetPort : IExternalNetworkPort, IEthernetPort, IVirtualSwitchPort, IVirtualizationManagementObject
{
}

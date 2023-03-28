namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_WiFiPort")]
internal interface IWiFiPort : IExternalNetworkPort, IEthernetPort, IVirtualSwitchPort, IVirtualizationManagementObject
{
}

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_EthernetSwitchOperationalData")]
internal interface IEthernetSwitchOperationalStatus : IEthernetSwitchStatus, IEthernetStatus, IVirtualizationManagementObject
{
}

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_SyntheticEthernetPort")]
internal interface ISyntheticEthernetPort : IEthernetPort, IVirtualSwitchPort, IVirtualizationManagementObject
{
}

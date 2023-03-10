namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_EmulatedEthernetPort")]
internal interface IEmulatedEthernetPort : IEthernetPort, IVirtualSwitchPort, IVirtualizationManagementObject
{
}

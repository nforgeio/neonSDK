namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_InternalEthernetPort")]
internal interface IInternalEthernetPort : IEthernetPort, IVirtualSwitchPort, IVirtualizationManagementObject, IPutable
{
}

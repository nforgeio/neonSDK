namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_FcSwitchPort")]
internal interface IVirtualFcSwitchPort : IVirtualSwitchPort, IVirtualizationManagementObject
{
	IFcEndpoint FcEndpoint { get; }

	IVirtualFcSwitch VirtualFcSwitch { get; }
}

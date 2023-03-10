namespace Microsoft.Virtualization.Client.Management;

[WmiName("CIM_NetworkPort")]
internal interface IVirtualSwitchPort : IVirtualizationManagementObject
{
	[Key]
	string Name { get; }

	string FriendlyName { get; }
}

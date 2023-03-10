namespace Microsoft.Virtualization.Client.Management;

internal interface IEthernetSwitchExtensionBase : IVirtualizationManagementObject
{
	string ExtensionId { get; }

	string FriendlyName { get; }

	EthernetSwitchExtensionType ExtensionType { get; }

	string Company { get; }

	string Version { get; }

	string Description { get; }
}

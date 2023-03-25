namespace Microsoft.Virtualization.Client.Management;

internal interface IExternalNetworkPort : IEthernetPort, IVirtualSwitchPort, IVirtualizationManagementObject
{
	bool IsBound { get; }

	bool Enabled { get; }

	IExternalEthernetPortCapabilities GetCapabilities();
}

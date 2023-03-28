namespace Microsoft.Virtualization.Client.Management;

internal interface IVMIntegrationComponent : IVMDevice, IVirtualizationManagementObject
{
	bool Enabled { get; }

	VMIntegrationComponentOperationalStatus[] GetOperationalStatus();

	string[] GetOperationalStatusDescriptions();
}

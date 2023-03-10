namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_ResourcePoolSettingData")]
internal interface IResourcePoolSetting : IVirtualizationManagementObject, IPutableAsync, IPutable
{
	string FriendlyName { get; set; }

	string PoolId { get; set; }

	IResourcePool ResourcePool { get; }

	string Notes { get; set; }
}

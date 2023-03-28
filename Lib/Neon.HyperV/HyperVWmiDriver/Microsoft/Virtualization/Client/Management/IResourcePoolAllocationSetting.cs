namespace Microsoft.Virtualization.Client.Management;

internal interface IResourcePoolAllocationSetting : IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable
{
	bool IsPoolRasd { get; set; }

	WmiObjectPath Parent { get; set; }

	IResourcePool ChildResourcePool { get; }

	IResourcePool ParentResourcePool { get; }
}

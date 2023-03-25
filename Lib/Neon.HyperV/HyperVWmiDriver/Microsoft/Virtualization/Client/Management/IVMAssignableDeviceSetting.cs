namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_PciExpressSettingData")]
internal interface IVMAssignableDeviceSetting : IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable, IDeleteableAsync, IDeleteable
{
	WmiObjectPath PhysicalDevicePath { get; set; }

	ushort VirtualFunction { get; set; }

	IVMAssignableDevice GetPhysicalDevice();
}

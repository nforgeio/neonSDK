using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_PciExpressSettingData", PrimaryMapping = false)]
internal interface IPciExpressPoolAllocationSetting : IResourcePoolAllocationSetting, IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable
{
	IEnumerable<IVMAssignableDevice> GetPciExpressDevices();

	void SetPciExpressDevices(IList<IVMAssignableDevice> pciExpressDevices);
}

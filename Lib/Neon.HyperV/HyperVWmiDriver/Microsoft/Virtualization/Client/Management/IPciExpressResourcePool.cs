using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_ResourcePool", PrimaryMapping = false)]
internal interface IPciExpressResourcePool : IResourcePool, IVirtualizationManagementObject, IDeleteableAsync, IDeleteable, IMetricMeasurableElement
{
	IEnumerable<IVMAssignableDevice> GetPciExpressDevices();

	bool HasPciExpressDevice(IVMAssignableDevice pciExpressDevice);
}

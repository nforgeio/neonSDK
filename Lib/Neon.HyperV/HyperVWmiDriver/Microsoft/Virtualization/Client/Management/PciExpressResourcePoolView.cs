using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Virtualization.Client.Management;

internal class PciExpressResourcePoolView : ResourcePoolView, IPciExpressResourcePool, IResourcePool, IVirtualizationManagementObject, IDeleteableAsync, IDeleteable, IMetricMeasurableElement
{
	public IEnumerable<IVMAssignableDevice> GetPciExpressDevices()
	{
		if (base.Primordial)
		{
			return GetRelatedObjects<IVMAssignableDevice>(base.Associations.ResourcePoolSystemComponents);
		}
		return base.AllocationSettings.Cast<IPciExpressPoolAllocationSetting>().SelectMany((IPciExpressPoolAllocationSetting setting) => setting.GetPciExpressDevices());
	}

	public bool HasPciExpressDevice(IVMAssignableDevice pciExpressDevice)
	{
		return GetPciExpressDevices().Any((IVMAssignableDevice pcieDevice) => pcieDevice.ManagementPath == pciExpressDevice.ManagementPath);
	}
}

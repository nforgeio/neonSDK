using System.Collections.Generic;
using System.Linq;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal static class PciExpressUtilities
{
	public static IEnumerable<IVMAssignableDevice> FilterAssignableDevices(IEnumerable<IVMAssignableDevice> devicesToFilter, string instancePath, string locationPath)
	{
		IEnumerable<IVMAssignableDevice> enumerable = devicesToFilter;
		if (!string.IsNullOrEmpty(instancePath))
		{
			enumerable = enumerable.Where((IVMAssignableDevice pciDevice) => pciDevice.DeviceInstancePath == instancePath);
		}
		if (!string.IsNullOrEmpty(locationPath))
		{
			enumerable = enumerable.Where((IVMAssignableDevice pciDevice) => pciDevice.LocationPath == locationPath);
		}
		return enumerable;
	}

	public static IEnumerable<VMAssignedDevice> FilterAssignableDevices(IEnumerable<VMAssignedDevice> devicesToFilter, string instancePath, string locationPath)
	{
		IEnumerable<VMAssignedDevice> enumerable = devicesToFilter;
		if (!string.IsNullOrEmpty(instancePath))
		{
			enumerable = enumerable.Where((VMAssignedDevice pciDevice) => pciDevice.InstanceID == instancePath);
		}
		if (!string.IsNullOrEmpty(locationPath))
		{
			enumerable = enumerable.Where((VMAssignedDevice pciDevice) => pciDevice.LocationPath == locationPath);
		}
		return enumerable;
	}
}

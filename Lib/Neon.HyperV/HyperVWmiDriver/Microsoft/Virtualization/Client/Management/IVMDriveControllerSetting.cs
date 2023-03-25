using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_ResourceAllocationSettingData", PrimaryMapping = false)]
internal interface IVMDriveControllerSetting : IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable
{
	string Address { get; }

	IEnumerable<IVMDriveSetting> GetDriveSettings();
}

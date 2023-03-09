using System;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_StorageAllocationSettingData")]
internal interface IVirtualDiskSetting : IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable, IDeleteableAsync, IDeleteable, IMetricMeasurableElement
{
	string Path { get; set; }

	IVMDriveSetting DriveSetting { get; set; }

	IVirtualDiskResourcePool ResourcePool { get; }

	ulong MinimumIOPS { get; set; }

	ulong MaximumIOPS { get; set; }

	Guid StorageQoSPolicyID { get; set; }

	bool PersistentReservationsSupported { get; set; }

	ushort WriteHardeningMethod { get; set; }
}

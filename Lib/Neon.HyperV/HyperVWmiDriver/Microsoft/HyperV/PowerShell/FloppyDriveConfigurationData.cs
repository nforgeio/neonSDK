using Microsoft.HyperV.PowerShell.Common;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class FloppyDriveConfigurationData : DriveConfigurationData
{
	internal override VMDeviceSettingType DiskSettingType => VMDeviceSettingType.FloppyDisk;

	internal override VMDeviceSettingType DriveSettingType => VMDeviceSettingType.DisketteSyntheticDrive;

	internal override bool HasAttachedDisk => base.AttachedDiskType != AttachedDiskType.None;

	internal override string DescriptionForDiskAttach => TaskDescriptions.SetVMFloppyDiskDrive;

	internal override string DescriptionForDriveAdd => TaskDescriptions.SetVMFloppyDiskDrive;

	internal FloppyDriveConfigurationData(VirtualMachineBase parent)
		: base(parent)
	{
	}

	internal override bool IsRemoveThenAddRequired(DriveConfigurationData oldConfiguration)
	{
		return false;
	}
}

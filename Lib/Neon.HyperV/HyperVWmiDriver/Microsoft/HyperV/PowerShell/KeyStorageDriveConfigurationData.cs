using Microsoft.HyperV.PowerShell.Common;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class KeyStorageDriveConfigurationData : DriveConfigurationData
{
    internal override VMDeviceSettingType DiskSettingType => VMDeviceSettingType.KeyStorageDrive;

    internal override VMDeviceSettingType DriveSettingType => VMDeviceSettingType.KeyStorageDrive;

    internal override bool HasAttachedDisk => false;

    internal override string DescriptionForDiskAttach => TaskDescriptions.SetVMDvdDrive_InsertDisk;

    internal override string DescriptionForDriveAdd => TaskDescriptions.AddVMDvdDrive;

    internal KeyStorageDriveConfigurationData(VirtualMachineBase parent)
        : base(parent)
    {
    }

    internal override void CopyValuesToDriveSetting(IVMDriveSetting settingToModify)
    {
        base.CopyValuesToDriveSetting(settingToModify);
        settingToModify.KSDConnectionPath = "size=42";
    }
}

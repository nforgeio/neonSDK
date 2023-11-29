using Microsoft.HyperV.PowerShell.Common;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class DvdDriveConfigurationData : DriveConfigurationData
{
    internal string PnpId { get; set; }

    internal override VMDeviceSettingType DiskSettingType => VMDeviceSettingType.IsoDisk;

    internal override VMDeviceSettingType DriveSettingType => VMDeviceSettingType.DvdSyntheticDrive;

    internal override bool HasAttachedDisk => base.AttachedDiskType != AttachedDiskType.None;

    internal override string DescriptionForDiskAttach => TaskDescriptions.SetVMDvdDrive_InsertDisk;

    internal override string DescriptionForDriveAdd => TaskDescriptions.AddVMDvdDrive;

    internal DvdDriveConfigurationData(VirtualMachineBase parent)
        : base(parent)
    {
    }

    public void SetRequestedPhysicalDrive(string volumeName)
    {
        IPhysicalCDRomDrive physicalCDRomDrive = PhysicalDriveUtilities.FindPhysicalDvdDrive(volumeName, base.Parent.Server, Constants.UpdateThreshold);
        PnpId = physicalCDRomDrive.PnpDeviceId;
    }

    public void CopyRequestedPhysicalDrive(DvdDriveConfigurationData originalConfiguration)
    {
        PnpId = originalConfiguration.PnpId;
    }

    internal override void CopyValuesToDiskSetting(IVirtualDiskSetting settingToModify)
    {
        if (base.AttachedDiskType == AttachedDiskType.Physical)
        {
            settingToModify.Path = PnpId;
        }
        else
        {
            base.CopyValuesToDiskSetting(settingToModify);
        }
    }
}

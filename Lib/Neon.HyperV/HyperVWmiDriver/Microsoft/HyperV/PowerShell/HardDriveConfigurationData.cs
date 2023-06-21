using System;
using Microsoft.HyperV.PowerShell.Common;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class HardDriveConfigurationData : DriveConfigurationData
{
    internal WmiObjectPath PhysicalDrivePath { get; set; }

    public bool? SupportPersistentReservations { get; set; }

    public ulong? MaximumIOPS { get; set; }

    public ulong? MinimumIOPS { get; set; }

    public Guid? QoSPolicyID { get; set; }

    public CacheAttributes? WriteHardeningMethod { get; set; }

    internal override VMDeviceSettingType DiskSettingType => VMDeviceSettingType.HardDisk;

    internal override VMDeviceSettingType DriveSettingType
    {
        get
        {
            if (base.AttachedDiskType != AttachedDiskType.Physical)
            {
                return VMDeviceSettingType.HardDiskSyntheticDrive;
            }
            return VMDeviceSettingType.HardDiskPhysicalDrive;
        }
    }

    internal override bool HasAttachedDisk => base.AttachedDiskType == AttachedDiskType.Virtual;

    internal override string DescriptionForDiskAttach => TaskDescriptions.SetVMHardDiskDrive_AttachVirtualDisk;

    internal override string DescriptionForDriveAdd => TaskDescriptions.AddVMHardDiskDrive;

    internal HardDriveConfigurationData(VirtualMachineBase parent)
        : base(parent)
    {
    }

    public void SetRequestedPhysicalDrive(uint diskNumber)
    {
        IVMHardDiskDrive iVMHardDiskDrive = PhysicalDriveUtilities.FindPhysicalHardDrive(diskNumber, base.Parent.Server, Constants.UpdateThreshold);
        PhysicalDrivePath = iVMHardDiskDrive.ManagementPath;
    }

    public void CopyRequestedPhysicalDrive(HardDriveConfigurationData originalConfiguration)
    {
        PhysicalDrivePath = originalConfiguration.PhysicalDrivePath;
    }

    internal override void CopyValuesToDriveSetting(IVMDriveSetting settingToModify)
    {
        base.CopyValuesToDriveSetting(settingToModify);
        if (base.AttachedDiskType == AttachedDiskType.Physical)
        {
            settingToModify.PhysicalPassThroughDrivePath = PhysicalDrivePath;
        }
    }

    internal override void CopyValuesToDiskSetting(IVirtualDiskSetting settingToModify)
    {
        base.CopyValuesToDiskSetting(settingToModify);
        if (MaximumIOPS.HasValue)
        {
            settingToModify.MaximumIOPS = MaximumIOPS.Value;
        }
        if (MinimumIOPS.HasValue)
        {
            settingToModify.MinimumIOPS = MinimumIOPS.Value;
        }
        if (QoSPolicyID.HasValue)
        {
            settingToModify.StorageQoSPolicyID = QoSPolicyID.Value;
        }
        if (SupportPersistentReservations.HasValue)
        {
            settingToModify.PersistentReservationsSupported = SupportPersistentReservations.Value;
        }
        if (WriteHardeningMethod.HasValue)
        {
            settingToModify.WriteHardeningMethod = (ushort)WriteHardeningMethod.Value;
        }
    }

    internal override bool IsRemoveThenAddRequired(DriveConfigurationData oldConfiguration)
    {
        bool flag = base.IsRemoveThenAddRequired(oldConfiguration);
        if (!flag)
        {
            bool num = oldConfiguration.AttachedDiskType == AttachedDiskType.Physical;
            bool flag2 = base.AttachedDiskType == AttachedDiskType.Physical;
            flag = num != flag2;
        }
        return flag;
    }
}

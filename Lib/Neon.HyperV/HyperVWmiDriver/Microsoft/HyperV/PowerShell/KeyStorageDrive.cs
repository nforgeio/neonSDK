using Microsoft.HyperV.PowerShell.Common;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class KeyStorageDrive : Drive
{
    internal override string PutDescription => TaskDescriptions.SetVMDvdDrive;

    protected override string DescriptionForDiskDetach => TaskDescriptions.SetVMDvdDrive_EjectDisk;

    protected override string DescriptionForDriveRemove => TaskDescriptions.RemoveVMDvdDrive;

    internal KeyStorageDrive(IVMDriveSetting setting, IVirtualDiskSetting attachedDiskSetting, VMDriveController driveController, VirtualMachineBase parentVirtualMachineObject)
        : base(setting, attachedDiskSetting, driveController, parentVirtualMachineObject)
    {
    }

    protected override DriveConfigurationData GetCurrentConfigurationSelf()
    {
        return new KeyStorageDriveConfigurationData(GetParentAs<VirtualMachineBase>());
    }
}

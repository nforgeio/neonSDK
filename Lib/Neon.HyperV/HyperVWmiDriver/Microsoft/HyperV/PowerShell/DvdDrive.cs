using Microsoft.HyperV.PowerShell.Common;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class DvdDrive : Drive
{
    internal override string PutDescription => TaskDescriptions.SetVMDvdDrive;

    protected override string DescriptionForDiskDetach => TaskDescriptions.SetVMDvdDrive_EjectDisk;

    protected override string DescriptionForDriveRemove => TaskDescriptions.RemoveVMDvdDrive;

    public DvdMediaType DvdMediaType => CalculateMediaTypeFromDiskDescriptor(Path);

    public override string Path
    {
        get
        {
            string path = base.Path;
            if (CalculateMediaTypeFromDiskDescriptor(path) == DvdMediaType.PassThrough)
            {
                return PhysicalDriveUtilities.FindPhysicalDvdDriveByPnpId(path, base.Server, Constants.UpdateThreshold).Drive;
            }
            return path;
        }
    }

    internal DvdDrive(IVMDriveSetting setting, IVirtualDiskSetting attachedDiskSetting, VMDriveController driveController, VirtualMachineBase parentVirtualMachineObject)
        : base(setting, attachedDiskSetting, driveController, parentVirtualMachineObject)
    {
    }

    private static DvdMediaType CalculateMediaTypeFromDiskDescriptor(string diskHostResource)
    {
        DvdMediaType result = DvdMediaType.None;
        if (!string.IsNullOrEmpty(diskHostResource))
        {
            result = (Utilities.IsIsoFilePath(diskHostResource) ? DvdMediaType.ISO : DvdMediaType.PassThrough);
        }
        return result;
    }

    protected override DriveConfigurationData GetCurrentConfigurationSelf()
    {
        DvdDriveConfigurationData dvdDriveConfigurationData = new DvdDriveConfigurationData(GetParentAs<VirtualMachineBase>());
        string text = m_AttachedDiskSetting.GetData(UpdatePolicy.EnsureUpdated)?.Path;
        AttachedDiskType attachedDiskType2 = (dvdDriveConfigurationData.AttachedDiskType = CalculateMediaTypeFromDiskDescriptor(text) switch
        {
            DvdMediaType.PassThrough => AttachedDiskType.Physical, 
            DvdMediaType.ISO => AttachedDiskType.Virtual, 
            _ => AttachedDiskType.None, 
        });
        if (attachedDiskType2 == AttachedDiskType.Physical)
        {
            dvdDriveConfigurationData.PnpId = text;
        }
        return dvdDriveConfigurationData;
    }
}

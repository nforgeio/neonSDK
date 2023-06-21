using System.Globalization;
using Microsoft.HyperV.PowerShell;
using Microsoft.HyperV.PowerShell.Common;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Virtualization.Client.Management;

using ErrorMessages = Microsoft.HyperV.PowerShell.Common.ErrorMessages;

namespace Microsoft.Vhd.PowerShell;

internal sealed class MountedDiskImage : VirtualizationObject
{
    private readonly IDataUpdater<IMountedStorageImage> m_MountedStorageImage;

    private uint? m_DiskNumber;

    [VirtualizationObjectIdentifier(IdentifierFlags.UniqueIdentifier | IdentifierFlags.FriendlyName)]
    internal string Path => m_MountedStorageImage.GetData(UpdatePolicy.EnsureUpdated).ImagePath;

    internal MountedDiskImage(IMountedStorageImage mountedStorageImage)
        : base(mountedStorageImage)
    {
        m_MountedStorageImage = InitializePrimaryDataUpdater(mountedStorageImage);
    }

    internal uint GetDiskNumber()
    {
        if (!m_DiskNumber.HasValue)
        {
            IWin32DiskDrive diskDrive = m_MountedStorageImage.GetData(UpdatePolicy.EnsureAssociatorsUpdated).GetDiskDrive();
            m_DiskNumber = diskDrive.DiskNumber;
        }
        return m_DiskNumber.Value;
    }

    internal void Dismount(IOperationWatcher operationWatcher)
    {
        IMountedStorageImage data = m_MountedStorageImage.GetData(UpdatePolicy.EnsureUpdated);
        operationWatcher.PerformOperation(data.BeginDetachVirtualHardDisk, data.EndDetachVirtualHardDisk, TaskDescriptions.DismountVHD, null);
    }

    internal static MountedDiskImage FindByPath(Server server, string diskPath)
    {
        if (!TryFindByPath(server, diskPath, out var image))
        {
            throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, ErrorMessages.VHD_MountedDiskImageNotFound, diskPath));
        }
        return image;
    }

    internal static bool TryFindByPath(Server server, string diskPath, out MountedDiskImage image)
    {
        IMountedStorageImage mountedStorageImage = ObjectLocator.GetImageManagementService(server).GetMountedStorageImage(diskPath);
        if (mountedStorageImage != null)
        {
            image = new MountedDiskImage(mountedStorageImage);
            return true;
        }
        image = null;
        return false;
    }

    internal static MountedDiskImage FindByDiskNumber(Server server, uint diskNumber)
    {
        IWin32DiskDrive obj = ObjectLocator.GetImageManagementService(server).GetWin32DiskDrive(diskNumber) ?? throw ExceptionHelper.CreateObjectNotFoundException(ErrorMessages.VHD_InvalidDiskNumberForVirtualHardDisk, null);
        obj.UpdatePropertyCache();
        return new MountedDiskImage(obj.GetMountedStorageImage() ?? throw ExceptionHelper.CreateObjectNotFoundException(ErrorMessages.VHD_InvalidDiskNumberForVirtualHardDisk, null));
    }
}

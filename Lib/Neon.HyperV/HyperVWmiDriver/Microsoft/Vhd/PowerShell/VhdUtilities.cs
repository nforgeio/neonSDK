using System.Collections.Generic;
using System.Linq;
using Microsoft.HyperV.PowerShell;
using Microsoft.HyperV.PowerShell.Common;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.Vhd.PowerShell;

internal static class VhdUtilities
{
	internal static void CreateVirtualFloppyDisk(Server server, string path, IOperationWatcher operationWatcher)
	{
		IImageManagementService service = ObjectLocator.GetImageManagementService(server);
		operationWatcher.PerformOperation(() => service.BeginCreateVirtualFloppyDisk(path), service.EndCreateVirtualFloppyDisk, TaskDescriptions.NewVFD, null);
	}

	internal static VirtualHardDisk CreateVirtualHardDisk(Server server, VhdType type, VhdFormat format, string path, string parentPath, long maxInternalSize, long blockSize, long logicalSectorSize, long physicalSectorSize, bool pmemCompatible, VirtualHardDiskPmemAddressAbstractionType addressAbstractionType, long dataAlignment, IOperationWatcher operationWatcher)
	{
		IImageManagementService service = ObjectLocator.GetImageManagementService(server);
		operationWatcher.PerformOperation(() => service.BeginCreateVirtualHardDisk((VirtualHardDiskType)type, (VirtualHardDiskFormat)format, path, parentPath, maxInternalSize, blockSize, logicalSectorSize, physicalSectorSize, pmemCompatible, addressAbstractionType, dataAlignment), service.EndCreateVirtualHardDisk, TaskDescriptions.NewVHD, null);
		return GetVirtualHardDisk(server, path, null);
	}

	internal static VirtualHardDisk ConvertVirtualHardDisk(Server server, string sourcePath, string destinationPath, string destinationParentPath, VhdType destinationType, VhdFormat destinationFormat, long blockSize, bool isPmemCompatible, VirtualHardDiskPmemAddressAbstractionType addressAbstractionType, IOperationWatcher operationWatcher)
	{
		IImageManagementService service = ObjectLocator.GetImageManagementService(server);
		operationWatcher.PerformOperation(() => service.BeginConvertVirtualHardDisk(sourcePath, destinationPath, destinationParentPath, (VirtualHardDiskType)destinationType, (VirtualHardDiskFormat)destinationFormat, blockSize, isPmemCompatible, addressAbstractionType), service.EndCreateVirtualHardDisk, TaskDescriptions.ConvertVHD, null);
		return GetVirtualHardDisk(server, destinationPath, null);
	}

	internal static void OptimizeVHDSet(Server server, string path, IOperationWatcher operationWatcher)
	{
		IImageManagementService service = ObjectLocator.GetImageManagementService(server);
		operationWatcher.PerformOperation(() => service.BeginOptimizeVHDSet(path), service.EndOptimizeVHDSet, TaskDescriptions.OptimizeVHDSet, null);
	}

	internal static void MountVirtualHardDisk(Server server, string diskPath, bool assignDriveLetter, bool readOnly, IOperationWatcher operationWatcher)
	{
		IImageManagementService service = ObjectLocator.GetImageManagementService(server);
		operationWatcher.PerformOperation(() => service.BeginAttachVirtualHardDisk(diskPath, assignDriveLetter, readOnly), service.EndAttachVirtualHardDisk, TaskDescriptions.MountVHD, null);
	}

	internal static void MergeVirtualHardDisk(Server server, string sourcePath, string destinationPath, IOperationWatcher operationWatcher)
	{
		IImageManagementService service = ObjectLocator.GetImageManagementService(server);
		operationWatcher.PerformOperation(() => service.BeginMergeVirtualHardDisk(sourcePath, destinationPath), service.EndMergeVirtualHardDisk, TaskDescriptions.MergeVHD, null);
	}

	internal static void CompactVirtualHardDisk(Server server, string path, VhdCompactMode mode, IOperationWatcher operationWatcher)
	{
		IImageManagementService service = ObjectLocator.GetImageManagementService(server);
		operationWatcher.PerformOperation(() => service.BeginCompactVirtualHardDisk(path, (uint)mode), service.EndCompactVirtualHardDisk, TaskDescriptions.OptimizeVHD, null);
	}

	internal static void ResizeVirtualHardDisk(Server server, string path, ulong? size, IOperationWatcher operationWatcher)
	{
		IImageManagementService service = ObjectLocator.GetImageManagementService(server);
		operationWatcher.PerformOperation(() => service.BeginResizeVirtualHardDisk(path, size), service.EndResizeVirtualHardDisk, TaskDescriptions.ResizeVHD, null);
	}

	internal static void SetVirtualHardDiskSettingData(Server server, string path, long physicalSectorSize, string diskId, IOperationWatcher operationWatcher)
	{
		IImageManagementService service = ObjectLocator.GetImageManagementService(server);
		VirtualHardDiskSettingData vhdSettingsData = new VirtualHardDiskSettingData(server, VirtualHardDiskType.Unknown, VirtualHardDiskFormat.Unknown, path, null)
		{
			PhysicalSectorSize = physicalSectorSize,
			VirtualDiskIdentifier = diskId
		};
		operationWatcher.PerformOperation(() => service.BeginSetVirtualHardDiskSettingData(vhdSettingsData), service.EndSetVirtualHardDiskSettingData, TaskDescriptions.SetVHD, null);
	}

	internal static void ReconnectParentVirtualHardDisk(Server server, string path, string parentPath, string leafPath, bool forceReconnect, IOperationWatcher operationWatcher)
	{
		IImageManagementService service = ObjectLocator.GetImageManagementService(server);
		operationWatcher.PerformOperation(() => service.BeginReconnectParentVirtualHardDisk(path, parentPath, leafPath, forceReconnect), service.EndReconnectParentVirtualHardDisk, TaskDescriptions.SetVHD, null);
	}

	internal static void ValidateVirtualHardDisk(Server server, string path, IOperationWatcher operationWatcher)
	{
		IImageManagementService service = ObjectLocator.GetImageManagementService(server);
		operationWatcher.PerformOperation(() => service.BeginValidateVirtualHardDisk(path), service.EndValidateVirtualHardDisk, TaskDescriptions.TestVHD, null);
	}

	internal static void ValidateSharedVirtualHardDisk(Server server, string path, IOperationWatcher operationWatcher)
	{
		IImageManagementService service = ObjectLocator.GetImageManagementService(server);
		operationWatcher.PerformOperation(() => service.BeginValidatePersistentReservationSupport(path), service.EndValidatePersistentReservationSupport, TaskDescriptions.TestVHD, null);
	}

	internal static void DeleteVHDSnapshot(Server server, string path, string snapshotId, bool persistReferenceSnapshot, IOperationWatcher operationWatcher)
	{
		IImageManagementService service = ObjectLocator.GetImageManagementService(server);
		operationWatcher.PerformOperation(() => service.BeginDeleteVHDSnapshot(path, snapshotId, persistReferenceSnapshot), service.EndDeleteVHDSnapshot, TaskDescriptions.DeleteVHDSnapshot, null);
	}

	internal static void SetVHDSnapshot(Server server, VHDSnapshotInfo snapshotInfo, IOperationWatcher operationWatcher)
	{
		IImageManagementService service = ObjectLocator.GetImageManagementService(server);
		operationWatcher.PerformOperation(() => service.BeginSetVHDSnapshotInformation(snapshotInfo.GetInformation()), service.EndSetVHDSnapshotInformation, TaskDescriptions.SetVHDSnapshot, null);
	}

	internal static VirtualHardDisk GetVirtualHardDisk(Server server, string path)
	{
		uint? diskNumber = null;
		if (MountedDiskImage.TryFindByPath(server, path, out var image))
		{
			diskNumber = image.GetDiskNumber();
		}
		return GetVirtualHardDisk(server, path, diskNumber);
	}

	internal static VirtualHardDisk GetVirtualHardDisk(Server server, string path, uint? diskNumber)
	{
		IImageManagementService imageManagementService = ObjectLocator.GetImageManagementService(server);
		VirtualHardDiskSettingData virtualHardDiskSettingData = imageManagementService.GetVirtualHardDiskSettingData(path);
		VirtualHardDiskState virtualHardDiskState = imageManagementService.GetVirtualHardDiskState(path);
		return new VirtualHardDisk(server, virtualHardDiskSettingData, virtualHardDiskState, diskNumber);
	}

	internal static VirtualHardDisk GetVirtualHardDiskByDiskNumber(Server server, uint diskNumber)
	{
		string path = MountedDiskImage.FindByDiskNumber(server, diskNumber).Path;
		return GetVirtualHardDisk(server, path, diskNumber);
	}

	internal static IReadOnlyList<VirtualHardDisk> GetVirtualHardDisks(Server server, IEnumerable<string> paths, IOperationWatcher operationWatcher)
	{
		return paths.SelectWithLogging((string path) => GetVirtualHardDisk(server, path), operationWatcher).ToList();
	}

	internal static VHDSetInfo GetVHDSetInfo(Server server, string path, bool getAllPaths)
	{
		return new VHDSetInfo(ObjectLocator.GetImageManagementService(server).GetVHDSetInformation(path, getAllPaths));
	}

	internal static IReadOnlyList<VHDSnapshotInfo> GetVHDSnapshotInfo(Server server, string path, List<string> snapshotIds, bool getParentPaths)
	{
		return (from information in ObjectLocator.GetImageManagementService(server).GetVHDSnapshotInformation(path, snapshotIds, getParentPaths)
			select new VHDSnapshotInfo(information)).ToList();
	}

	internal static string GetDeviceIdByDiskNumber(Server server, uint diskNumber)
	{
		string result = null;
		IWin32DiskDrive win32DiskDrive = ObjectLocator.GetImageManagementService(server).GetWin32DiskDrive(diskNumber);
		if (win32DiskDrive != null)
		{
			result = win32DiskDrive.DeviceId;
		}
		return result;
	}
}

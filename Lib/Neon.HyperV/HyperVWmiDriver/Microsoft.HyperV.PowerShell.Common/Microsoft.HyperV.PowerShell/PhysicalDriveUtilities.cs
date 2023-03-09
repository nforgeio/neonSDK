using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.HyperV.PowerShell.Common;
using Microsoft.Virtualization.Client.Management;

using ErrorMessages = Microsoft.HyperV.PowerShell.Common.ErrorMessages;

namespace Microsoft.HyperV.PowerShell;

internal static class PhysicalDriveUtilities
{
	internal static IVMHardDiskDrive FindPhysicalHardDrive(uint diskNumber, Server server, TimeSpan updateThreshold)
	{
		IVMHardDiskDrive iVMHardDiskDrive = GetPhysicalHardDrives(server, updateThreshold).FirstOrDefault((IVMHardDiskDrive drive) => drive.PhysicalDiskNumber == diskNumber);
		if (iVMHardDiskDrive == null)
		{
			throw ExceptionHelper.CreateObjectNotFoundException(ErrorMessages.PhysicalHardDrive_NotFound, null);
		}
		return iVMHardDiskDrive;
	}

	private static IEnumerable<IVMHardDiskDrive> GetPhysicalHardDrives(Server server, TimeSpan updateThreshold)
	{
		IResourcePool primordialResourcePool = ObjectLocator.GetHostComputerSystem(server).GetPrimordialResourcePool(VMDeviceSettingType.HardDiskPhysicalDrive);
		primordialResourcePool.UpdateAssociationCache(updateThreshold);
		return primordialResourcePool.PhysicalDevices.Cast<IVMHardDiskDrive>();
	}

	internal static IPhysicalCDRomDrive FindPhysicalDvdDrive(string volumeName, Server server, TimeSpan updateThreshold)
	{
		IPhysicalCDRomDrive physicalCDRomDrive = GetPhysicalDvdDrives(server, updateThreshold).FirstOrDefault((IPhysicalCDRomDrive drive) => string.Equals(drive.Drive, volumeName, StringComparison.OrdinalIgnoreCase));
		if (physicalCDRomDrive == null)
		{
			throw ExceptionHelper.CreateObjectNotFoundException(ErrorMessages.PhysicalDvdDrive_NotFound, null);
		}
		return physicalCDRomDrive;
	}

	internal static IPhysicalCDRomDrive FindPhysicalDvdDriveByPnpId(string plugAndPlayId, Server server, TimeSpan updateThreshold)
	{
		IPhysicalCDRomDrive physicalCDRomDrive = GetPhysicalDvdDrives(server, updateThreshold).FirstOrDefault((IPhysicalCDRomDrive drive) => string.Equals(drive.PnpDeviceId, plugAndPlayId, StringComparison.OrdinalIgnoreCase));
		if (physicalCDRomDrive == null)
		{
			throw ExceptionHelper.CreateObjectNotFoundException(ErrorMessages.PhysicalDvdDrive_NotFound, null);
		}
		return physicalCDRomDrive;
	}

	private static IEnumerable<IPhysicalCDRomDrive> GetPhysicalDvdDrives(Server server, TimeSpan updateThreshold)
	{
		return ObjectLocator.GetHostComputerSystem(server).GetPhysicalCDDrives(update: true, updateThreshold);
	}
}

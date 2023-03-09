using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.Virtualization.Client;

internal static class ImportFileUtilities
{
	public static List<IVirtualDiskSetting> GetVhdsForVM(IVMComputerSystemBase virtualMachine)
	{
		return (from device in new List<IVMComputerSystemSetting>(virtualMachine.Snapshots) { virtualMachine.Setting }.SelectMany((IVMComputerSystemSetting setting) => setting.GetDeviceSettings())
			where device.VMDeviceSettingType == VMDeviceSettingType.HardDisk
			select device).OfType<IVirtualDiskSetting>().ToList();
	}

	public static string CreateStagingFile(Server server, string destinationFolderPath)
	{
		IImageManagementService imageManagementService = ObjectLocator.GetImageManagementService(server);
		string randomFileName = Path.GetRandomFileName();
		string text = Path.Combine(destinationFolderPath, randomFileName + ".vhdx");
		using IVMTask iVMTask = imageManagementService.BeginCreateVirtualHardDisk(VirtualHardDiskType.DynamicallyExpanding, VirtualHardDiskFormat.Vhdx, text, null, VirtualHardDiskConstants.MinimumSizeInBytes);
		iVMTask.WaitForCompletion();
		imageManagementService.EndCreateVirtualHardDisk(iVMTask);
		return text;
	}

	public static Dictionary<string, string> FindStorageFiles(Server server, IEnumerable<IVirtualDiskSetting> diskSettings, string sourceFolderPath, bool fallbackToOriginalPath)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		IImageManagementService imageManagementService = ObjectLocator.GetImageManagementService(server);
		foreach (string item in diskSettings.Select((IVirtualDiskSetting disk) => disk.Path))
		{
			string text = item;
			while (!string.IsNullOrEmpty(text))
			{
				string fileName = Path.GetFileName(text);
				string text2 = Path.Combine(sourceFolderPath, fileName);
				if (dictionary.ContainsKey(fileName))
				{
					break;
				}
				string text3 = null;
				if (FileUtilities.FileExists(server, text2))
				{
					text3 = text2;
				}
				else
				{
					if (!fallbackToOriginalPath || !FileUtilities.FileExists(server, text))
					{
						break;
					}
					text3 = text;
				}
				dictionary.Add(fileName, text3);
				text = imageManagementService.GetVirtualHardDiskSettingData(text3).ParentPath;
			}
		}
		return dictionary;
	}

	public static void StageStorageCopy(Server server, string destinationFolderPath, Dictionary<string, string> sourceFiles, List<string> destinationFiles)
	{
		if (sourceFiles == null)
		{
			throw new ArgumentNullException("sourceFiles");
		}
		string text = CreateStagingFile(server, destinationFolderPath);
		try
		{
			foreach (KeyValuePair<string, string> sourceFile in sourceFiles)
			{
				string key = sourceFile.Key;
				string text2 = Path.Combine(destinationFolderPath, key);
				FileUtilities.CopyFile(server, text, text2, overwrite: false);
				destinationFiles.Add(text2);
			}
		}
		finally
		{
			FileUtilities.DeleteFile(server, text);
		}
	}

	public static void CleanupStorageFiles(Server server, IEnumerable<string> storageFiles)
	{
		foreach (string storageFile in storageFiles)
		{
			FileUtilities.DeleteFile(server, storageFile);
		}
	}

	public static string GetNewVhdPathForHardDisk(string newFolder, IVirtualDiskSetting diskSetting)
	{
		diskSetting.UpdatePropertyCache(TimeSpan.FromSeconds(2.0));
		string fileName = Path.GetFileName(diskSetting.Path);
		return Path.Combine(newFolder, fileName);
	}

	public static void UpdateVhdPathForHardDisk(string newFolder, IVirtualDiskSetting diskSetting)
	{
		string newVhdPathForHardDisk = GetNewVhdPathForHardDisk(newFolder, diskSetting);
		if (!string.Equals(newVhdPathForHardDisk, diskSetting.Path, StringComparison.OrdinalIgnoreCase))
		{
			diskSetting.Path = newVhdPathForHardDisk;
			diskSetting.Put();
		}
	}
}

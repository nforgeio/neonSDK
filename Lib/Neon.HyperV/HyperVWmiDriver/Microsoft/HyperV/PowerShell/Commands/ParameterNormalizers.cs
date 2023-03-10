using Microsoft.HyperV.PowerShell.ExtensionMethods;

namespace Microsoft.HyperV.PowerShell.Commands;

internal static class ParameterNormalizers
{
	internal static void NormalizeMoveOrCompareVMParameters(IMoveOrCompareVMCmdlet moveOrCompareVmCmdlet, string currentFileSystemLocation)
	{
		if (!string.IsNullOrEmpty(moveOrCompareVmCmdlet.VirtualMachinePath))
		{
			moveOrCompareVmCmdlet.VirtualMachinePath = PathUtility.GetFullPath(moveOrCompareVmCmdlet.VirtualMachinePath, currentFileSystemLocation);
		}
		if (!string.IsNullOrEmpty(moveOrCompareVmCmdlet.SnapshotFilePath))
		{
			moveOrCompareVmCmdlet.SnapshotFilePath = PathUtility.GetFullPath(moveOrCompareVmCmdlet.SnapshotFilePath, currentFileSystemLocation);
		}
		if (!string.IsNullOrEmpty(moveOrCompareVmCmdlet.SmartPagingFilePath))
		{
			moveOrCompareVmCmdlet.SmartPagingFilePath = PathUtility.GetFullPath(moveOrCompareVmCmdlet.SmartPagingFilePath, currentFileSystemLocation);
		}
		if (!moveOrCompareVmCmdlet.Vhds.IsNullOrEmpty())
		{
			moveOrCompareVmCmdlet.VhdMigrationMappings = VhdMigrationMapping.CreateMappingsFromHashtable(moveOrCompareVmCmdlet.Vhds);
		}
	}

	internal static void NormalizeImportOrCompareVMParameters(IImportOrCompareVMCmdlet importOrCompareVmCmdlet, string currentFileSystemLocation)
	{
		if (!string.IsNullOrEmpty(importOrCompareVmCmdlet.Path))
		{
			importOrCompareVmCmdlet.Path = PathUtility.GetFullPath(importOrCompareVmCmdlet.Path, currentFileSystemLocation);
		}
		if (!string.IsNullOrEmpty(importOrCompareVmCmdlet.VirtualMachinePath))
		{
			importOrCompareVmCmdlet.VirtualMachinePath = PathUtility.GetFullPath(importOrCompareVmCmdlet.VirtualMachinePath, currentFileSystemLocation);
		}
		if (!string.IsNullOrEmpty(importOrCompareVmCmdlet.SnapshotFilePath))
		{
			importOrCompareVmCmdlet.SnapshotFilePath = PathUtility.GetFullPath(importOrCompareVmCmdlet.SnapshotFilePath, currentFileSystemLocation);
		}
		if (!string.IsNullOrEmpty(importOrCompareVmCmdlet.SmartPagingFilePath))
		{
			importOrCompareVmCmdlet.SmartPagingFilePath = PathUtility.GetFullPath(importOrCompareVmCmdlet.SmartPagingFilePath, currentFileSystemLocation);
		}
		if (!string.IsNullOrEmpty(importOrCompareVmCmdlet.VhdDestinationPath))
		{
			importOrCompareVmCmdlet.VhdDestinationPath = PathUtility.GetFullPath(importOrCompareVmCmdlet.VhdDestinationPath, currentFileSystemLocation);
		}
		if (!string.IsNullOrEmpty(importOrCompareVmCmdlet.VhdSourcePath))
		{
			importOrCompareVmCmdlet.VhdSourcePath = PathUtility.GetFullPath(importOrCompareVmCmdlet.VhdSourcePath, currentFileSystemLocation);
		}
	}
}

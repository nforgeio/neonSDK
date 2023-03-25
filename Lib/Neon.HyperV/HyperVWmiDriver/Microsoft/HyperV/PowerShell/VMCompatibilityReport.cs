using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Microsoft.HyperV.PowerShell.Common;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Virtualization.Client;
using Microsoft.Virtualization.Client.Management;

using ErrorMessages = Microsoft.HyperV.PowerShell.Common.ErrorMessages;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMCompatibilityReport
{
	public VirtualMachine VM { get; private set; }

	public OperationType OperationType { get; private set; }

	public string Destination { get; private set; }

	public string Path { get; private set; }

	public string SnapshotPath { get; internal set; }

	public string VhdDestinationPath { get; internal set; }

	public string VhdSourcePath { get; internal set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	public VMCompatibilityError[] Incompatibilities { get; private set; }

	internal bool ImportRegistersVm { get; private set; }

	internal IReadOnlyList<VhdMigrationMapping> VhdMigrationMappings { get; private set; }

	internal string VhdResourcePoolName { get; private set; }

	internal VMCompatibilityReport(VirtualMachine pvm, OperationType operation, string destination, string path, string snapshotPath, string vhdDestinationPath, string vhdSourcePath, IReadOnlyList<VhdMigrationMapping> vhdMigrationMappings, string resourcePoolName, IEnumerable<MsvmError> errors)
	{
		VM = pvm;
		OperationType = operation;
		Destination = destination;
		Path = path;
		SnapshotPath = snapshotPath;
		VhdDestinationPath = vhdDestinationPath;
		VhdSourcePath = vhdSourcePath;
		VhdMigrationMappings = vhdMigrationMappings;
		VhdResourcePoolName = resourcePoolName;
		Incompatibilities = errors.Select((MsvmError error) => new VMCompatibilityError(pvm, error)).ToArray();
	}

	private static bool TryToDeletePlannedVM(VirtualMachine pvm, IOperationWatcher operationWatcher)
	{
		bool result = false;
		try
		{
			((IRemovable)pvm).Remove(operationWatcher);
			result = true;
			return result;
		}
		catch (Exception e)
		{
			ExceptionHelper.DisplayErrorOnException(e, operationWatcher);
			return result;
		}
	}

	private static bool TryToDeletePlannedVMForFile(Server server, string path, IOperationWatcher operationWatcher)
	{
		if (!Guid.TryParseExact(global::System.IO.Path.GetFileNameWithoutExtension(path), "D", out var result))
		{
			throw ExceptionHelper.CreateInvalidArgumentException(ErrorMessages.InvalidParameter_FileNameNotGuid);
		}
		bool result2 = false;
		if (VirtualizationObjectLocator.TryGetVirtualMachineById(server, result, out var virtualMachine) && virtualMachine.IsPlanned && !virtualMachine.IsDeleted)
		{
			result2 = TryToDeletePlannedVM(virtualMachine, operationWatcher);
		}
		return result2;
	}

	private static VMCompatibilityReport CreateReportForImport(VirtualMachine pvm, bool importRegistersVm, string path, string snapshotSourcePath, string vhdDestinationPath, string vhdSourcePath, IOperationWatcher operationWatcher)
	{
		Server server = pvm.Server;
		List<string> list = new List<string>();
		try
		{
			if (!vhdDestinationPath.IsNullOrEmpty())
			{
				List<IVirtualDiskSetting> vhdsForVM = ImportFileUtilities.GetVhdsForVM(pvm.GetComputerSystem(UpdatePolicy.EnsureUpdated));
				if (!vhdSourcePath.IsNullOrEmpty() && !string.Equals(vhdSourcePath, vhdDestinationPath, StringComparison.OrdinalIgnoreCase))
				{
					Dictionary<string, string> sourceFiles = ImportFileUtilities.FindStorageFiles(server, vhdsForVM, vhdSourcePath, fallbackToOriginalPath: true);
					ImportFileUtilities.StageStorageCopy(server, vhdDestinationPath, sourceFiles, list);
				}
				foreach (IVirtualDiskSetting item in vhdsForVM)
				{
					ImportFileUtilities.UpdateVhdPathForHardDisk(vhdDestinationPath, item);
				}
			}
			IVMService service = ObjectLocator.GetVirtualizationService(pvm.Server);
			IVMPlannedComputerSystem pvmObject = (IVMPlannedComputerSystem)pvm.GetComputerSystem(UpdatePolicy.None);
			List<MsvmError> errors = operationWatcher.PerformOperationWithReturn(() => service.BeginValidatePlannedVirtualSystem(pvmObject), service.EndValidatePlannedVirtualSystem, TaskDescriptions.Task_ValidateVM, pvm);
			return new VMCompatibilityReport(pvm, OperationType.ImportVirtualMachine, pvm.Server.UserSpecifiedName, path, snapshotSourcePath, vhdDestinationPath, vhdSourcePath, null, null, errors)
			{
				ImportRegistersVm = importRegistersVm
			};
		}
		finally
		{
			ImportFileUtilities.CleanupStorageFiles(server, list);
		}
	}

	internal static VMCompatibilityReport CreateReportForRegisterImport(Server server, string path, IOperationWatcher operationWatcher)
	{
		string directoryName = global::System.IO.Path.GetDirectoryName(path);
		string text = global::System.IO.Path.GetDirectoryName(directoryName);
		if (string.IsNullOrEmpty(text))
		{
			text = directoryName;
		}
		string text2 = global::System.IO.Path.Combine(text, "Snapshots");
		TryToDeletePlannedVMForFile(server, path, operationWatcher);
		VirtualMachine virtualMachine = VirtualMachine.Import(server, path, text2, generateNewId: false, operationWatcher);
		VMCompatibilityReport vMCompatibilityReport = null;
		try
		{
			string vhdDestinationPath = null;
			List<IVirtualDiskSetting> vhdsForVM = ImportFileUtilities.GetVhdsForVM(virtualMachine.GetComputerSystem(UpdatePolicy.EnsureUpdated));
			string standardVhdFolderPath = global::System.IO.Path.Combine(text, "Virtual Hard Disks");
			if (Utilities.DirectoryExists(server, standardVhdFolderPath) && vhdsForVM.All((IVirtualDiskSetting disk) => Utilities.FileExists(server, ImportFileUtilities.GetNewVhdPathForHardDisk(standardVhdFolderPath, disk))))
			{
				vhdDestinationPath = standardVhdFolderPath;
			}
			virtualMachine.Path = text;
			virtualMachine.SnapshotFileLocation = text;
			virtualMachine.SmartPagingFilePath = text;
			((IUpdatable)virtualMachine).Put(operationWatcher);
			return CreateReportForImport(virtualMachine, importRegistersVm: true, path, text2, vhdDestinationPath, null, operationWatcher);
		}
		catch (Exception)
		{
			TryToDeletePlannedVM(virtualMachine, operationWatcher);
			throw;
		}
	}

	internal static VMCompatibilityReport CreateReportForCopyImport(Server server, string path, bool generateNewId, string virtualMachinePath, string snapshotPath, string smartPagingFilePath, string vhdDestinationPath, string vhdSourcePath, IOperationWatcher operationWatcher)
	{
		IVMService virtualizationService = ObjectLocator.GetVirtualizationService(server);
		if (virtualMachinePath.IsNullOrEmpty())
		{
			virtualMachinePath = virtualizationService.Setting.DefaultExternalDataRoot;
		}
		string directoryName = global::System.IO.Path.GetDirectoryName(path);
		string text = global::System.IO.Path.GetDirectoryName(directoryName);
		if (string.IsNullOrEmpty(text))
		{
			text = directoryName;
		}
		string text2 = global::System.IO.Path.Combine(text, "Snapshots");
		if (vhdSourcePath.IsNullOrEmpty())
		{
			string text3 = global::System.IO.Path.Combine(text, "Virtual Hard Disks");
			if (Utilities.DirectoryExists(server, text3))
			{
				vhdSourcePath = text3;
			}
		}
		if (vhdDestinationPath.IsNullOrEmpty())
		{
			vhdDestinationPath = virtualizationService.Setting.DefaultVirtualHardDiskPath;
		}
		if (!generateNewId)
		{
			TryToDeletePlannedVMForFile(server, path, operationWatcher);
		}
		VirtualMachine virtualMachine = VirtualMachine.Import(server, path, text2, generateNewId, operationWatcher);
		VMCompatibilityReport vMCompatibilityReport = null;
		try
		{
			virtualMachine.Path = virtualMachinePath;
			virtualMachine.SnapshotFileLocation = snapshotPath;
			virtualMachine.SmartPagingFilePath = smartPagingFilePath;
			((IUpdatable)virtualMachine).Put(operationWatcher);
			return CreateReportForImport(virtualMachine, importRegistersVm: false, path, text2, vhdDestinationPath, vhdSourcePath, operationWatcher);
		}
		catch (Exception)
		{
			TryToDeletePlannedVM(virtualMachine, operationWatcher);
			throw;
		}
	}

	internal static VMCompatibilityReport RegenerateReportForImport(VMCompatibilityReport report, IOperationWatcher operationWatcher)
	{
		return CreateReportForImport(report.VM, report.ImportRegistersVm, report.Path, report.SnapshotPath, report.VhdDestinationPath, report.VhdSourcePath, operationWatcher);
	}
}

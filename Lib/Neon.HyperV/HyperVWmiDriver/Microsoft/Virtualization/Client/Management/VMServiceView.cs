#define TRACE
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Management.Infrastructure;

namespace Microsoft.Virtualization.Client.Management;

internal class VMServiceView : EthernetSwitchFeatureServiceView, IVMService, IVirtualizationManagementObject, IPutable, IEthernetSwitchFeatureService
{
	internal static class WmiMemberNames
	{
		public const string ImportSystemDefinition = "ImportSystemDefinition";

		public const string ImportSnapshotDefinitions = "ImportSnapshotDefinitions";

		public const string RealizePlannedSystem = "RealizePlannedSystem";

		public const string ValidatePlannedSystem = "ValidatePlannedSystem";

		public const string ExportSystemDefinition = "ExportSystemDefinition";

		public const string CreateVirtualSystem = "DefineSystem";

		public const string CreatePlannedSystem = "DefinePlannedSystem";

		public const string GenerateWwpn = "GenerateWwpn";

		public const string GetDefinitionFileSummaryInformation = "GetDefinitionFileSummaryInformation";

		public const string AddBootSourceSettings = "AddBootSourceSettings";

		public const string SecureBootTemplateInstance = "Microsoft:Definition\\VirtualSystem\\SecureBootTemplateId\\{0}";
	}

	private class VMServiceErrorCodeMapper : ErrorCodeMapper
	{
		private readonly string m_ImportFile;

		public VMServiceErrorCodeMapper(string importFile)
		{
			m_ImportFile = importFile;
		}

		public override string MapError(VirtualizationOperation operation, long errorCode, string operationFailedMsg)
		{
			if (operation == VirtualizationOperation.ImportSystemDefinition && errorCode == -2)
			{
				return string.Format(CultureInfo.InvariantCulture, ErrorMessages.ImportComputerSystemSuccededButObjectPathNotFound, m_ImportFile);
			}
			if (operation == VirtualizationOperation.ExportVirtualSystem && errorCode == -2)
			{
				return ErrorMessages.CreateVirtualSystemSuccededButObjectPathNotFound;
			}
			return base.MapError(operation, errorCode, operationFailedMsg);
		}
	}

	private string m_ImportFile;

	public IVMServiceSetting Setting => GetRelatedObject<IVMServiceSetting>(base.Associations.ElementSettingData);

	public IVMServiceCapabilities AllCapabilities => GetRelatedObject<IVMServiceCapabilities>(base.Associations.ElementCapabilities);

	public IVMComputerSystemSetting GetSettingsCapabilities(SettingsDefineCapabilities capability)
	{
		if (capability == null)
		{
			throw new ArgumentNullException("capability");
		}
		IVMComputerSystemSetting iVMComputerSystemSetting = null;
		foreach (IVMComputerSystemSetting computerSystemSetting in AllCapabilities.ComputerSystemSettings)
		{
			if (capability.MatchByDescription(computerSystemSetting.InstanceId))
			{
				iVMComputerSystemSetting = computerSystemSetting;
				break;
			}
		}
		if (iVMComputerSystemSetting == null)
		{
			throw ThrowHelper.CreateRelatedObjectNotFoundException(base.Server, typeof(IVMComputerSystemSetting));
		}
		return iVMComputerSystemSetting;
	}

	public IVMTask BeginImportSystemDefinition(string systemDefinition, string snapshotFolder, bool generateNewId)
	{
		if (string.IsNullOrEmpty(systemDefinition))
		{
			throw new ArgumentException(null, "systemDefinition");
		}
		string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.ImportComputerSystemFailed, systemDefinition);
		m_ImportFile = systemDefinition;
		VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Starting import system definition from {0}", systemDefinition));
		object[] array = new object[5] { systemDefinition, snapshotFolder, generateNewId, null, null };
		uint result = InvokeMethod("ImportSystemDefinition", array);
		IVMTask iVMTask = BeginMethodTaskReturn(result, array[3], array[4]);
		iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
		return iVMTask;
	}

	public IVMPlannedComputerSystem EndImportSystemDefinition(IVMTask task, bool returnImportedVM)
	{
		IVMPlannedComputerSystem result = null;
		if (returnImportedVM)
		{
			result = EndMethodReturn<IVMPlannedComputerSystem>(task, VirtualizationOperation.ImportSystemDefinition);
		}
		else
		{
			EndMethod(task, VirtualizationOperation.ImportSystemDefinition);
		}
		VMTrace.TraceUserActionCompleted("Import virtual system completed successfully.");
		return result;
	}

	public IVMTask BeginImportSnapshotDefinitions(IVMPlannedComputerSystem pvm, string snapshotLocation)
	{
		if (pvm == null)
		{
			throw new ArgumentNullException("pvm");
		}
		if (string.IsNullOrEmpty(snapshotLocation))
		{
			throw new ArgumentNullException("snapshotLocation");
		}
		string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.ImportSnapshotDefinitionsFailed, snapshotLocation);
		VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Starting import snapshots from {0}", snapshotLocation));
		object[] array = new object[4] { pvm, snapshotLocation, null, null };
		uint result = InvokeMethod("ImportSnapshotDefinitions", array);
		IVMTask iVMTask = BeginMethodTaskReturn(result, array[2], array[3]);
		iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
		return iVMTask;
	}

	public IEnumerable<IVMComputerSystemSetting> EndImportSnapshotDefinitions(IVMTask task, bool returnSnapshots)
	{
		if (task == null)
		{
			throw new ArgumentNullException("task");
		}
		IEnumerable<IVMComputerSystemSetting> result = null;
		if (returnSnapshots)
		{
			result = EndMethodReturnEnumeration<IVMComputerSystemSetting>(task, VirtualizationOperation.ImportSnapshotDefinitions);
		}
		else
		{
			EndMethod(task, VirtualizationOperation.ImportSnapshotDefinitions);
		}
		VMTrace.TraceUserActionCompleted("Import snapshots completed successfully.");
		return result;
	}

	public IVMTask BeginExportSystemDefinition(IVMComputerSystem computerSystem, string exportDirectory, IVMExportSetting exportSetting)
	{
		if (computerSystem == null)
		{
			throw new ArgumentNullException("computerSystem");
		}
		if (string.IsNullOrEmpty(exportDirectory))
		{
			throw new ArgumentException(null, "exportDirectory");
		}
		string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.ExportComputerSystemFailed, computerSystem.Name, exportDirectory);
		VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Starting export virtual system ex '{0}' to dir '{1}'", computerSystem.ManagementPath.ToString(), exportDirectory));
		string text = exportSetting?.GetEmbeddedInstance();
		object[] array = new object[4] { computerSystem, exportDirectory, text, null };
		uint result = InvokeMethod("ExportSystemDefinition", array);
		IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[3]);
		iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
		return iVMTask;
	}

	public void EndExportSystemDefinition(IVMTask task)
	{
		EndMethod(task, VirtualizationOperation.ExportVirtualSystem);
		VMTrace.TraceUserActionCompleted("Export virtual system completed successfully.");
	}

	public IVMTask BeginValidatePlannedVirtualSystem(IVMPlannedComputerSystem plannedComputerSystem)
	{
		if (plannedComputerSystem == null)
		{
			throw new ArgumentNullException("plannedComputerSystem");
		}
		string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.ValidatePlannedComputerSystemFailed, plannedComputerSystem.Name);
		VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Starting Validate of {0}", plannedComputerSystem.ManagementPath.ToString()));
		object[] array = new object[2] { plannedComputerSystem, null };
		uint result = InvokeMethod("ValidatePlannedSystem", array);
		IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[1]);
		iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
		return iVMTask;
	}

	public List<MsvmError> EndValidatePlannedVirtualSystem(IVMTask task)
	{
		EndMethod(task, VirtualizationOperation.ValidatePlannedSystem);
		VMTrace.TraceUserActionCompleted("Validate planned system completed successfully.");
		return ((IVMVirtualizationTask)task).GetErrors();
	}

	public IVMTask BeginRealizePlannedVirtualSystem(IVMPlannedComputerSystem plannedComputerSystem)
	{
		if (plannedComputerSystem == null)
		{
			throw new ArgumentNullException("plannedComputerSystem");
		}
		string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.RealizePlannedComputerSystemFailed, plannedComputerSystem.Name);
		VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Starting Realize of {0}", plannedComputerSystem.ManagementPath.ToString()));
		object[] array = new object[3] { plannedComputerSystem, null, null };
		uint result = InvokeMethod("RealizePlannedSystem", array);
		IVMTask iVMTask = BeginMethodTaskReturn(result, array[1], array[2]);
		iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
		return iVMTask;
	}

	public IVMComputerSystem EndRealizePlannedVirtualSystem(IVMTask task, bool returnRealizedVm)
	{
		IVMComputerSystem result = null;
		if (returnRealizedVm)
		{
			result = EndMethodReturn<IVMComputerSystem>(task, VirtualizationOperation.RealizePlannedSystem);
		}
		else
		{
			EndMethod(task, VirtualizationOperation.RealizePlannedSystem);
		}
		VMTrace.TraceUserActionCompleted("Realize planned system completed successfully.");
		return result;
	}

	public List<ISummaryInformationSnapshot> GetDefinitionFileSummaryInformation(string[] paths)
	{
		List<ISummaryInformationSnapshot> list = new List<ISummaryInformationSnapshot>();
		object[] array = new object[2] { paths, null };
		uint num = GetServiceProxy().InvokeMethod("GetDefinitionFileSummaryInformation", array);
		if (num == View.ErrorCodeSuccess && array[1] != null)
		{
			foreach (ICimInstance item in ((CimInstance[])array[1]).ToICimInstances())
			{
				list.Add(new SummaryInformationSnapshot(item));
			}
			return list;
		}
		if (array[1] != null)
		{
			VMTrace.TraceError("GetDefinitionFileSummaryInformation WMI method call failed!");
		}
		throw ThrowHelper.CreateVirtualizationOperationFailedException(string.Empty, VirtualizationOperation.GetDefinitionFileSummaryInformation, num);
	}

	public IVMTask BeginCreateVirtualSystem(string name, string location, VirtualSystemSubType generation, Version version = null)
	{
		IVMComputerSystemSetting settingsCapabilities = GetSettingsCapabilities(Capabilities.DefaultCapability);
		settingsCapabilities.Name = name;
		settingsCapabilities.ConfigurationDataRoot = location;
		settingsCapabilities.SnapshotDataRoot = location;
		settingsCapabilities.SwapFileDataRoot = location;
		settingsCapabilities.VirtualSystemSubType = generation;
		if (version != null)
		{
			settingsCapabilities.Version = version.ToString(2);
			if (version < VMConfigurationVersion.WinThreshold_0)
			{
				settingsCapabilities.UserSnapshotType = UserSnapshotType.Standard;
			}
		}
		return BeginCreateVirtualSystem(settingsCapabilities, Enumerable.Empty<IVMDeviceSetting>());
	}

	public IVMTask BeginCreateVirtualSystem(IVMComputerSystemSetting systemSetting, IEnumerable<IVMDeviceSetting> resourceSettings)
	{
		return BeginCreateVirtualSystem(systemSetting, resourceSettings, null);
	}

	public IVMTask BeginCreateVirtualSystem(IVMComputerSystemSetting systemSetting, IEnumerable<IVMDeviceSetting> resourceSettings, IVMComputerSystemSetting referenceSetting)
	{
		string embeddedInstance = systemSetting.GetEmbeddedInstance();
		object[] array = new object[5]
		{
			embeddedInstance,
			resourceSettings.Select((IVMDeviceSetting setting) => setting.GetEmbeddedInstance()).ToArray(),
			referenceSetting,
			null,
			null
		};
		VMTrace.TraceUserActionInitiated("Starting create virtual system", embeddedInstance);
		uint result = InvokeMethod("DefineSystem", array);
		IVMTask iVMTask = BeginMethodTaskReturn(result, array[3], array[4]);
		iVMTask.ClientSideFailedMessage = ErrorMessages.CreateVirtualSystemFailed;
		return iVMTask;
	}

	public T EndCreateVirtualSystem<T>(IVMTask task) where T : class, IVMComputerSystemBase
	{
		T result = EndMethodReturn<T>(task, VirtualizationOperation.CreateVirtualSystem);
		VMTrace.TraceUserActionCompleted("Creating virtual system completed successfully.");
		return result;
	}

	public IVMTask BeginCreatePlannedComputerSystem(IVMComputerSystemSetting systemSetting, IEnumerable<IVMDeviceSetting> resourceSettings, IVMComputerSystemSetting referenceSetting)
	{
		string embeddedInstance = systemSetting.GetEmbeddedInstance();
		object[] array = new object[5]
		{
			embeddedInstance,
			resourceSettings.Select((IVMDeviceSetting setting) => setting.GetEmbeddedInstance()).ToArray(),
			referenceSetting,
			null,
			null
		};
		VMTrace.TraceUserActionInitiated("Starting create planned virtual system", embeddedInstance);
		uint result = InvokeMethod("DefinePlannedSystem", array);
		IVMTask iVMTask = BeginMethodTaskReturn(result, array[3], array[4]);
		iVMTask.ClientSideFailedMessage = ErrorMessages.CreatePlannedSystemFailed;
		return iVMTask;
	}

	public IVMPlannedComputerSystem EndCreatePlannedComputerSystem(IVMTask task)
	{
		IVMPlannedComputerSystem result = EndMethodReturn<IVMPlannedComputerSystem>(task, VirtualizationOperation.CreatePlannedSystem);
		VMTrace.TraceUserActionCompleted("Creating planned virtual system completed successfully.");
		return result;
	}

	public string[] GenerateWorldWidePortNames(int count)
	{
		if (count <= 0)
		{
			throw new ArgumentException(null, "count");
		}
		VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Generating World Wide Names."));
		object[] array = new object[3] { count, null, null };
		uint num = InvokeMethod("GenerateWwpn", array);
		if (num != View.ErrorCodeSuccess)
		{
			throw ThrowHelper.CreateVirtualizationOperationFailedException(string.Format(CultureInfo.CurrentCulture, ErrorMessages.GeneratingWorldWideNamesFailed), VirtualizationOperation.GenerateWorldWideNames, num);
		}
		return (string[])array[1];
	}

	public Task<IVMBootEntry[]> AddBootSourceSettingsAsync(IVMComputerSystemBase computerSystem, BootSourceSetting[] bootSourceSettings)
	{
		object[] args = new object[4]
		{
			computerSystem,
			bootSourceSettings.Select((BootSourceSetting s) => s.ToString()).ToArray(),
			null,
			null
		};
		VMTrace.TraceUserActionInitiated("Starting add boot source settings");
		return Task.Run(delegate
		{
			uint result = InvokeMethod("AddBootSourceSettings", args);
			IVMTask iVMTask = BeginMethodTaskReturn(result, args[2], args[3]);
			iVMTask.ClientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.AddBootSourceFailed, computerSystem.Name);
			IVMBootEntry[] result2 = EndMethodReturnEnumeration<IVMBootEntry>(iVMTask, VirtualizationOperation.AddBootSourceSetting).ToArray();
			VMTrace.TraceUserActionCompleted("Adding boot source settings completed successfully.");
			return result2;
		});
	}

	public IEnumerable<string> GetSupportedVmVersions()
	{
		return AllCapabilities.SupportedVersionSettings.Select((IVMComputerSystemSetting s) => s.Version);
	}

	public IEnumerable<SecureBootTemplate> GetSecureBootTemplates()
	{
		return AllCapabilities.SupportedSecureBootTemplateSettings.Select((IVMComputerSystemSetting setting) => new SecureBootTemplate(setting.SecureBootTemplateId.Value, setting.Name, setting.Description));
	}

	public bool TryGetSecureBootTemplate(Guid id, out SecureBootTemplate template)
	{
		string instanceId = string.Format(CultureInfo.InvariantCulture, "Microsoft:Definition\\VirtualSystem\\SecureBootTemplateId\\{0}", id.ToString("D"));
		IVMComputerSystemSetting iVMComputerSystemSetting;
		try
		{
			iVMComputerSystemSetting = ObjectLocator.GetVMComputerSystemSetting(base.Server, instanceId);
		}
		catch (ObjectNotFoundException)
		{
			iVMComputerSystemSetting = null;
		}
		if (iVMComputerSystemSetting != null && iVMComputerSystemSetting.SecureBootTemplateId.HasValue)
		{
			template = new SecureBootTemplate(iVMComputerSystemSetting.SecureBootTemplateId.Value, iVMComputerSystemSetting.Name, iVMComputerSystemSetting.Description);
			return true;
		}
		template = null;
		return false;
	}

	protected override ErrorCodeMapper GetErrorCodeMapper()
	{
		return new VMServiceErrorCodeMapper(m_ImportFile);
	}
}

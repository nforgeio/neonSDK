using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.HyperV.PowerShell.Common;
using Microsoft.Virtualization.Client.Management;

using ErrorMessages = Microsoft.HyperV.PowerShell.Common.ErrorMessages;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMRemoteFx3DVideoAdapter : VMDevice, IRemovable
{
	private const uint gm_RdvhRoleID = 322u;

	private readonly DataUpdater<IVMSynthetic3DDisplayControllerSetting> m_AdapterSetting;

	public string MaximumScreenResolution
	{
		get
		{
			return RemoteFXUtilities.AllResolutions[NumberConverter.Int32ToByte(m_AdapterSetting.GetData(UpdatePolicy.EnsureUpdated).MaximumScreenResolution)].ToString();
		}
		internal set
		{
			int num = RemoteFXUtilities.AllResolutions.FindIndex((Synth3DResolution obj) => obj.ToString().Equals(value, StringComparison.OrdinalIgnoreCase));
			if (num >= 0)
			{
				m_AdapterSetting.GetData(UpdatePolicy.None).MaximumScreenResolution = num;
			}
		}
	}

	public byte MaximumMonitors
	{
		get
		{
			return NumberConverter.Int32ToByte(m_AdapterSetting.GetData(UpdatePolicy.EnsureUpdated).MaximumMonitors);
		}
		internal set
		{
			m_AdapterSetting.GetData(UpdatePolicy.None).MaximumMonitors = value;
		}
	}

	public ulong VRAMSizeBytes
	{
		get
		{
			return m_AdapterSetting.GetData(UpdatePolicy.EnsureUpdated).VRAMSizeBytes;
		}
		internal set
		{
			m_AdapterSetting.GetData(UpdatePolicy.None).VRAMSizeBytes = value;
		}
	}

	internal override string PutDescription => TaskDescriptions.SetVMRemoteFx3DVideoAdapter;

	internal VMRemoteFx3DVideoAdapter(IVMSynthetic3DDisplayControllerSetting setting, VirtualMachineBase parentVirtualMachineObject)
		: base(setting, parentVirtualMachineObject)
	{
		m_AdapterSetting = InitializePrimaryDataUpdater(setting);
	}

	internal override IDataUpdater<IVMDeviceSetting> GetDeviceDataUpdater()
	{
		return m_AdapterSetting;
	}

	void IRemovable.Remove(IOperationWatcher operationWatcher)
	{
		IVMSynthetic3DDisplayControllerSetting data = m_AdapterSetting.GetData(UpdatePolicy.None);
		RemoveInternal(data, TaskDescriptions.RemoveVMRemoteFx3DVideoAdapter, operationWatcher);
	}

	internal static VMRemoteFx3DVideoAdapter AddSynthetic3DDisplayController(VirtualMachine vm, IOperationWatcher operationWatcher)
	{
		ISynth3dVideoResourcePool synthetic3DDisplayControllerResourcePool = GetSynthetic3DDisplayControllerResourcePool(vm.Server);
		if (!synthetic3DDisplayControllerResourcePool.IsGPUCapable)
		{
			throw ExceptionHelper.CreateOperationFailedException(ErrorMessages.VMRemoteFx3DVideoAdapter_NotGpuCapable);
		}
		if (vm.GetSynthetic3DDisplayController() != null)
		{
			throw ExceptionHelper.CreateOperationFailedException(ErrorMessages.VMRemoteFx3DVideoAdapter_AdapterAlreadyExists);
		}
		VMVideo.RemoveSyntheticDisplayController(vm, operationWatcher);
		VMRemoteFx3DVideoAdapter vMRemoteFx3DVideoAdapter;
		try
		{
			vMRemoteFx3DVideoAdapter = PrivateAddSynthetic3DDisplayController(vm, synthetic3DDisplayControllerResourcePool, operationWatcher);
		}
		catch (VirtualizationOperationFailedException)
		{
			VMVideo.AddSyntheticDisplayController(vm, operationWatcher);
			throw;
		}
		if (vm.Generation == 1)
		{
			try
			{
				S3DisplayController s3DisplayController = vm.GetS3DisplayController();
				s3DisplayController.Address = "02C1,00000000,01";
				((IUpdatable)s3DisplayController).Put(operationWatcher);
				return vMRemoteFx3DVideoAdapter;
			}
			catch (VirtualizationOperationFailedException)
			{
				PrivateRemoveSynthetic3DDisplayController(vMRemoteFx3DVideoAdapter, operationWatcher);
				VMVideo.AddSyntheticDisplayController(vm, operationWatcher);
				throw;
			}
		}
		return vMRemoteFx3DVideoAdapter;
	}

	internal static void RemoveSynthetic3DDisplayController(VMRemoteFx3DVideoAdapter adapter, IOperationWatcher operationWatcher)
	{
		VirtualMachine virtualMachine = adapter.GetParentAs<VirtualMachineBase>().GetVirtualMachine();
		PrivateRemoveSynthetic3DDisplayController(adapter, operationWatcher);
		try
		{
			VMVideo.AddSyntheticDisplayController(virtualMachine, operationWatcher);
		}
		catch (VirtualizationOperationFailedException)
		{
			PrivateAddSynthetic3DDisplayController(virtualMachine, GetSynthetic3DDisplayControllerResourcePool(virtualMachine.Server), operationWatcher);
			throw;
		}
		if (virtualMachine.Generation == 1)
		{
			try
			{
				S3DisplayController s3DisplayController = virtualMachine.GetS3DisplayController();
				s3DisplayController.Address = "5353,00000000,00";
				((IUpdatable)s3DisplayController).Put(operationWatcher);
			}
			catch (VirtualizationOperationFailedException)
			{
				VMVideo.RemoveSyntheticDisplayController(virtualMachine, operationWatcher);
				PrivateAddSynthetic3DDisplayController(virtualMachine, GetSynthetic3DDisplayControllerResourcePool(virtualMachine.Server), operationWatcher);
				throw;
			}
		}
	}

	internal string[] RetrieveMonitorCounts()
	{
		return new string[2]
		{
			RemoteFXUtilities.AllMonitorCounts.First().ToString(CultureInfo.InvariantCulture),
			RemoteFXUtilities.AllMonitorCounts.Last().ToString(CultureInfo.InvariantCulture)
		};
	}

	internal string RetrieveResolutions()
	{
		Version version = new Version(GetParentAs<VirtualMachine>().Version);
		List<Synth3DResolution> source = RemoteFXUtilities.RetrieveResolutionList(MaximumMonitors, version.Major);
		return string.Join(", ", source.Select((Synth3DResolution obj) => obj.ToString()).ToArray());
	}

	internal string RetrieveVramSizes(bool mappingBased = true)
	{
		int monitorIndex = 0;
		int resolutionIndex = 0;
		if (mappingBased)
		{
			Synth3DResolution newResolution = null;
			Synth3DResolution.TryParse(MaximumScreenResolution, out newResolution);
			monitorIndex = RemoteFXUtilities.AllMonitorCounts.IndexOf(MaximumMonitors);
			resolutionIndex = RemoteFXUtilities.AllResolutions.FindIndex((Synth3DResolution obj) => obj.CompareTo(newResolution) == 0);
		}
		List<Synth3DVramSize> source = RemoteFXUtilities.RetrievePrunedVramList(RemoteFXUtilities.RetrieveMinRequiredVramSize(monitorIndex, resolutionIndex, VMConfigurationVersion.WinThreshold_0.Major));
		return string.Join(", ", source.Select((Synth3DVramSize obj) => obj.VramSize.ToString(CultureInfo.InvariantCulture)).ToArray());
	}

	internal bool IsMonitorCountInRange()
	{
		int num = RemoteFXUtilities.AllMonitorCounts.IndexOf(MaximumMonitors);
		return -1 != num;
	}

	internal bool IsResolutionInRange(string resolution, bool mappingBased = true)
	{
		Version version = new Version(GetParentAs<VirtualMachine>().Version);
		byte monitorCount = MaximumMonitors;
		if (!mappingBased)
		{
			monitorCount = RemoteFXUtilities.AllMonitorCounts.First();
		}
		if (Synth3DResolution.TryParse(resolution, out var newResolution))
		{
			int num = RemoteFXUtilities.RetrieveResolutionList(monitorCount, version.Major).FindIndex((Synth3DResolution obj) => obj.CompareTo(newResolution) == 0);
			return -1 != num;
		}
		return false;
	}

	internal bool IsVramSizeEnough()
	{
		Version version = new Version(GetParentAs<VirtualMachine>().Version);
		ulong num = 4294967295uL;
		if (Synth3DResolution.TryParse(MaximumScreenResolution, out var newResolution))
		{
			int monitorIndex = RemoteFXUtilities.AllMonitorCounts.IndexOf(MaximumMonitors);
			int resolutionIndex = RemoteFXUtilities.RetrieveResolutionList(MaximumMonitors, version.Major).FindIndex((Synth3DResolution obj) => obj.CompareTo(newResolution) == 0);
			num = RemoteFXUtilities.RetrieveMinRequiredVramSize(monitorIndex, resolutionIndex, version.Major);
		}
		return VRAMSizeBytes >= num;
	}

	internal bool IsVramSizeInRange()
	{
		return -1 != RemoteFXUtilities.AllVRAMSizes.FindIndex((Synth3DVramSize obj) => obj.CompareTo(VRAMSizeBytes) == 0);
	}

	internal bool IsPermissibleToSetVram()
	{
		return new Version(GetParentAs<VirtualMachine>().Version).Major >= VMConfigurationVersion.WinThreshold_0.Major;
	}

	internal string GetVMConfigurationVersion()
	{
		return GetParentAs<VirtualMachine>().Version;
	}

	internal bool AdjustVramSize()
	{
		Synth3DResolution newResolution = null;
		Synth3DResolution.TryParse(MaximumScreenResolution, out newResolution);
		int num = RemoteFXUtilities.AllMonitorCounts.IndexOf(MaximumMonitors);
		int num2 = RemoteFXUtilities.AllResolutions.FindIndex((Synth3DResolution obj) => obj.CompareTo(newResolution) == 0);
		if (num2 < 0 || num < 0)
		{
			return false;
		}
		ulong vRAMSizeBytes = RemoteFXUtilities.RetrieveMinRequiredVramSize(num, num2, VMConfigurationVersion.WinThreshold_0.Major);
		VRAMSizeBytes = vRAMSizeBytes;
		return true;
	}

	private static VMRemoteFx3DVideoAdapter PrivateAddSynthetic3DDisplayController(VirtualMachine vm, ISynth3dVideoResourcePool synthetic3DResourcePool, IOperationWatcher operationWatcher)
	{
		IVMSynthetic3DDisplayControllerSetting templateSetting = (IVMSynthetic3DDisplayControllerSetting)synthetic3DResourcePool.GetCapabilities(Capabilities.DefaultCapability);
		return new VMRemoteFx3DVideoAdapter(vm.AddDeviceSetting(templateSetting, TaskDescriptions.AddVMRemoteFx3dVideoAdapter, operationWatcher), vm);
	}

	private static ISynth3dVideoResourcePool GetSynthetic3DDisplayControllerResourcePool(Server host)
	{
		ISynth3dVideoResourcePool result;
		try
		{
			result = (ISynth3dVideoResourcePool)ObjectLocator.GetHostComputerSystem(host).GetPrimordialResourcePool(VMDeviceSettingType.Synth3dVideo);
		}
		catch (ObjectNotFoundException innerException)
		{
			throw ExceptionHelper.CreateObjectNotFoundException(ErrorMessages.VMRemoteFx3DVideoAdapter_Synth3dVideoPoolNotFound, innerException);
		}
		if (!host.IsClientSku && host.QueryInstances(Server.CimV2Namespace, string.Format(CultureInfo.InvariantCulture, "SELECT * FROM {0} WHERE ID = {1}", "Win32_ServerFeature", 322u)).FirstOrDefault() == null)
		{
			throw ExceptionHelper.CreateObjectNotFoundException(ErrorMessages.VMRemoteFx3DVideoAdapter_DevNotInstalledError, null);
		}
		return result;
	}

	private static void PrivateRemoveSynthetic3DDisplayController(VMRemoteFx3DVideoAdapter adapterToRemove, IOperationWatcher operationWatcher)
	{
		((IRemovable)adapterToRemove)?.Remove(operationWatcher);
	}
}

using Microsoft.HyperV.PowerShell.Common;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMVideo : VMDevice, IRemovable
{
	private readonly DataUpdater<IVMSyntheticDisplayControllerSetting> m_ControllerSetting;

	internal override string PutDescription => TaskDescriptions.SetVMVideo;

	public ResolutionType ResolutionType
	{
		get
		{
			return (ResolutionType)m_ControllerSetting.GetData(UpdatePolicy.EnsureUpdated).ResolutionType;
		}
		internal set
		{
			m_ControllerSetting.GetData(UpdatePolicy.None).ResolutionType = (Microsoft.Virtualization.Client.Management.ResolutionType)value;
		}
	}

	public int HorizontalResolution
	{
		get
		{
			return m_ControllerSetting.GetData(UpdatePolicy.EnsureUpdated).HorizontalResolution;
		}
		internal set
		{
			m_ControllerSetting.GetData(UpdatePolicy.None).HorizontalResolution = value;
		}
	}

	public int VerticalResolution
	{
		get
		{
			return m_ControllerSetting.GetData(UpdatePolicy.EnsureUpdated).VerticalResolution;
		}
		internal set
		{
			m_ControllerSetting.GetData(UpdatePolicy.None).VerticalResolution = value;
		}
	}

	internal VMVideo(IVMSyntheticDisplayControllerSetting setting, VirtualMachineBase parentVirtualMachineObject)
		: base(setting, parentVirtualMachineObject)
	{
		m_ControllerSetting = InitializePrimaryDataUpdater(setting);
	}

	internal override IDataUpdater<IVMDeviceSetting> GetDeviceDataUpdater()
	{
		return m_ControllerSetting;
	}

	void IRemovable.Remove(IOperationWatcher operationWatcher)
	{
		IVMSyntheticDisplayControllerSetting data = m_ControllerSetting.GetData(UpdatePolicy.None);
		RemoveInternal(data, TaskDescriptions.RemoveVMHIDDevices, operationWatcher);
	}

	internal static VMVideo AddSyntheticDisplayController(VirtualMachine vm, IOperationWatcher operationWatcher)
	{
		IVMSyntheticDisplayControllerSetting templateSetting = (IVMSyntheticDisplayControllerSetting)ObjectLocator.GetHostComputerSystem(vm.Server).GetPrimordialResourcePool(VMDeviceSettingType.SynthVideo).GetCapabilities(Capabilities.DefaultCapability);
		return new VMVideo(vm.AddDeviceSetting(templateSetting, TaskDescriptions.AddVMSyntheticDisplayController, operationWatcher), vm);
	}

	internal static void RemoveSyntheticDisplayController(VirtualMachine vm, IOperationWatcher operationWatcher)
	{
		((IRemovable)vm.GetSyntheticDisplayController())?.Remove(operationWatcher);
	}
}

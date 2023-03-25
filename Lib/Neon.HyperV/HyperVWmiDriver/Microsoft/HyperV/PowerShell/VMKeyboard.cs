using Microsoft.HyperV.PowerShell.Common;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMKeyboard : VMDevice, IRemovable
{
	private readonly DataUpdater<IVMSyntheticKeyboardControllerSetting> m_KeyboardSetting;

	internal override string PutDescription => TaskDescriptions.SetVMKeyboard;

	internal VMKeyboard(IVMSyntheticKeyboardControllerSetting setting, VirtualMachineBase parentVirtualMachineObject)
		: base(setting, parentVirtualMachineObject)
	{
		m_KeyboardSetting = InitializePrimaryDataUpdater(setting);
	}

	internal override IDataUpdater<IVMDeviceSetting> GetDeviceDataUpdater()
	{
		return m_KeyboardSetting;
	}

	void IRemovable.Remove(IOperationWatcher operationWatcher)
	{
		IVMSyntheticKeyboardControllerSetting data = m_KeyboardSetting.GetData(UpdatePolicy.None);
		RemoveInternal(data, TaskDescriptions.RemoveVMHIDDevices, operationWatcher);
	}

	internal static VMKeyboard AddSyntheticKeyboardController(VirtualMachine vm, IOperationWatcher operationWatcher)
	{
		IVMSyntheticKeyboardControllerSetting templateSetting = VMDevice.CreateTemplateDeviceSetting<IVMSyntheticKeyboardControllerSetting>(vm.Server, VMDeviceSettingType.SynthKeyboard);
		return new VMKeyboard(vm.AddDeviceSetting(templateSetting, TaskDescriptions.AddVMSyntheticKeyboardController, operationWatcher), vm);
	}
}

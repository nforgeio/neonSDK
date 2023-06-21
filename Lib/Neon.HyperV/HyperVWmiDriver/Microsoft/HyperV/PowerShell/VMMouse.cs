using Microsoft.HyperV.PowerShell.Common;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMMouse : VMDevice, IRemovable
{
    private readonly DataUpdater<IVMSyntheticMouseControllerSetting> m_MouseSetting;

    internal override string PutDescription => TaskDescriptions.SetVMMouse;

    internal VMMouse(IVMSyntheticMouseControllerSetting setting, VirtualMachineBase parentVirtualMachineObject)
        : base(setting, parentVirtualMachineObject)
    {
        m_MouseSetting = InitializePrimaryDataUpdater(setting);
    }

    internal override IDataUpdater<IVMDeviceSetting> GetDeviceDataUpdater()
    {
        return m_MouseSetting;
    }

    void IRemovable.Remove(IOperationWatcher operationWatcher)
    {
        IVMSyntheticMouseControllerSetting data = m_MouseSetting.GetData(UpdatePolicy.None);
        RemoveInternal(data, TaskDescriptions.RemoveVMHIDDevices, operationWatcher);
    }

    internal static VMMouse AddSyntheticMouseController(VirtualMachine vm, IOperationWatcher operationWatcher)
    {
        IVMSyntheticMouseControllerSetting templateSetting = VMDevice.CreateTemplateDeviceSetting<IVMSyntheticMouseControllerSetting>(vm.Server, VMDeviceSettingType.SynthMouse);
        return new VMMouse(vm.AddDeviceSetting(templateSetting, TaskDescriptions.AddVMSyntheticMouseController, operationWatcher), vm);
    }
}

using Microsoft.HyperV.PowerShell.Common;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMBattery : VMDevice, IRemovable
{
    private readonly DataUpdater<IVMBatterySetting> m_BatterySetting;

    internal override string PutDescription => TaskDescriptions.SetVMBattery;

    internal VMBattery(IVMBatterySetting setting, ComputeResource parentComputeResource)
        : base(setting, parentComputeResource)
    {
        m_BatterySetting = InitializePrimaryDataUpdater(setting);
    }

    internal override IDataUpdater<IVMDeviceSetting> GetDeviceDataUpdater()
    {
        return m_BatterySetting;
    }

    void IRemovable.Remove(IOperationWatcher operationWatcher)
    {
        IVMBatterySetting data = m_BatterySetting.GetData(UpdatePolicy.None);
        RemoveInternal(data, TaskDescriptions.RemoveVMBattery, operationWatcher);
    }

    internal static VMBattery AddBattery(VirtualMachine vm, IOperationWatcher operationWatcher)
    {
        IVMBatterySetting templateSetting = VMDevice.CreateTemplateDeviceSetting<IVMBatterySetting>(vm.Server, VMDeviceSettingType.Battery);
        return new VMBattery(vm.AddDeviceSetting(templateSetting, TaskDescriptions.AddVMBattery, operationWatcher), vm);
    }
}

using Microsoft.HyperV.PowerShell.Common;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMPmemController : VMDriveController, IAddableVMDevice<IVMPmemControllerSetting>, IAddable, IRemovable
{
    private readonly int m_ControllerNumber;

    public override int ControllerNumber => m_ControllerNumber;

    internal override int ControllerLocationCount => 128;

    internal override ControllerType ControllerType => ControllerType.PMEM;

    internal override string PutDescription => TaskDescriptions.SetVMPmemController;

    public bool IsTemplate { get; private set; }

    public string DescriptionForAdd => TaskDescriptions.AddVMPmemController;

    internal VMPmemController(IVMDriveControllerSetting setting, int controllerNumber, VirtualMachineBase parentVirtualMachineObject)
        : base(setting, parentVirtualMachineObject)
    {
        m_ControllerNumber = controllerNumber;
    }

    public IVMPmemControllerSetting GetDeviceSetting(UpdatePolicy policy)
    {
        return m_ControllerSetting.GetDataAs<IVMPmemControllerSetting>(policy);
    }

    public void FinishAddingDeviceSetting(IVMPmemControllerSetting deviceSetting)
    {
    }

    void IRemovable.Remove(IOperationWatcher operationWatcher)
    {
        IVMPmemControllerSetting deviceSetting = (IVMPmemControllerSetting)m_ControllerSetting.GetData(UpdatePolicy.None);
        RemoveInternal(deviceSetting, TaskDescriptions.RemoveVMPmemController, operationWatcher);
    }

    internal static VMPmemController CreateTemplatePmemController(VirtualMachine parentVirtualMachine)
    {
        return new VMPmemController((IVMPmemControllerSetting)ObjectLocator.GetHostComputerSystem(parentVirtualMachine.Server).GetSettingCapabilities(VMDeviceSettingType.PmemController, Capabilities.DefaultCapability), -1, parentVirtualMachine)
        {
            IsTemplate = true
        };
    }
}

using Microsoft.HyperV.PowerShell.Common;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMScsiController : VMDriveController, IAddableVMDevice<IVMScsiControllerSetting>, IAddable, IRemovable
{
    private readonly int m_ControllerNumber;

    public override int ControllerNumber => m_ControllerNumber;

    internal override int ControllerLocationCount => 64;

    internal override ControllerType ControllerType => ControllerType.SCSI;

    internal override string PutDescription => TaskDescriptions.SetVMScsiController;

    public bool IsTemplate { get; private set; }

    string IAddableVMDevice<IVMScsiControllerSetting>.DescriptionForAdd => TaskDescriptions.AddVMScsiController;

    internal VMScsiController(IVMScsiControllerSetting setting, int controllerNumber, VirtualMachineBase parentVirtualMachineObject)
        : base(setting, parentVirtualMachineObject)
    {
        m_ControllerNumber = controllerNumber;
    }

    void IRemovable.Remove(IOperationWatcher operationWatcher)
    {
        IVMScsiControllerSetting deviceSetting = (IVMScsiControllerSetting)m_ControllerSetting.GetData(UpdatePolicy.None);
        RemoveInternal(deviceSetting, TaskDescriptions.RemoveVMScsiController, operationWatcher);
    }

    void IAddableVMDevice<IVMScsiControllerSetting>.FinishAddingDeviceSetting(IVMScsiControllerSetting deviceSetting)
    {
    }

    IVMScsiControllerSetting IAddableVMDevice<IVMScsiControllerSetting>.GetDeviceSetting(UpdatePolicy policy)
    {
        return m_ControllerSetting.GetDataAs<IVMScsiControllerSetting>(policy);
    }

    internal static VMScsiController CreateTemplateScsiController(VirtualMachine parentVirtualMachine)
    {
        return new VMScsiController((IVMScsiControllerSetting)ObjectLocator.GetHostComputerSystem(parentVirtualMachine.Server).GetSettingCapabilities(VMDeviceSettingType.ScsiSyntheticController, Capabilities.DefaultCapability), -1, parentVirtualMachine)
        {
            IsTemplate = true
        };
    }
}

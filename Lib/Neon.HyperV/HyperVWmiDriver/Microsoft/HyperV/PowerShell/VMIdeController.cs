using Microsoft.HyperV.PowerShell.Common;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMIdeController : VMDriveController
{
    internal override string PutDescription => TaskDescriptions.SetVMIdeController;

    internal override int ControllerLocationCount => 2;

    internal override ControllerType ControllerType => ControllerType.IDE;

    public override int ControllerNumber
    {
        get
        {
            string address = m_ControllerSetting.GetData(UpdatePolicy.EnsureUpdated).Address;
            if (!string.IsNullOrEmpty(address) && int.TryParse(address, out var result))
            {
                return result;
            }
            return -1;
        }
    }

    internal VMIdeController(IVMDriveControllerSetting setting, VirtualMachineBase parentVirtualMachineObject)
        : base(setting, parentVirtualMachineObject)
    {
    }
}

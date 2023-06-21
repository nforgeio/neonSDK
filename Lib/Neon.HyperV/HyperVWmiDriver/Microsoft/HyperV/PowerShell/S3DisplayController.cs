using Microsoft.HyperV.PowerShell.Common;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class S3DisplayController : VMDevice
{
    private readonly DataUpdater<IVMS3DisplayControllerSetting> m_ControllerSetting;

    public string Address
    {
        get
        {
            return m_ControllerSetting.GetData(UpdatePolicy.EnsureUpdated).Address;
        }
        internal set
        {
            m_ControllerSetting.GetData(UpdatePolicy.None).Address = value;
        }
    }

    internal override string PutDescription => TaskDescriptions.SetVMS3DisplayController;

    internal S3DisplayController(IVMS3DisplayControllerSetting setting, VirtualMachineBase parentVirtualMachineObject)
        : base(setting, parentVirtualMachineObject)
    {
        m_ControllerSetting = InitializePrimaryDataUpdater(setting);
    }

    internal override IDataUpdater<IVMDeviceSetting> GetDeviceDataUpdater()
    {
        return m_ControllerSetting;
    }
}

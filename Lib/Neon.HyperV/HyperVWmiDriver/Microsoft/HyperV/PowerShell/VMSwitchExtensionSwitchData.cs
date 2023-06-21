using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMSwitchExtensionSwitchData : VMSwitchExtensionRuntimeData
{
    private readonly VMSwitch m_ParentSwitch;

    public string SwitchName => m_ParentSwitch.Name;

    internal VMSwitchExtensionSwitchData(IEthernetSwitchStatus switchStatus, VMSwitch parentSwitch)
        : base(switchStatus)
    {
        m_ParentSwitch = parentSwitch;
    }
}

using System;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMInternalNetworkAdapter : VMInternalOrExternalNetworkAdapter
{
    private readonly IDataUpdater<IInternalEthernetPort> m_InternalNetworkAdapter;

    public override bool IsManagementOs => true;

    public string MacAddress => m_InternalNetworkAdapter.GetData(UpdatePolicy.EnsureUpdated)?.PermanentAddress;

    public string DeviceId => m_InternalNetworkAdapter.GetData(UpdatePolicy.EnsureUpdated)?.DeviceId?.Replace("Microsoft:", string.Empty);

    public override string AdapterId => m_InternalNetworkAdapter.GetData(UpdatePolicy.EnsureUpdated)?.Name;

    internal VMInternalNetworkAdapter(IVirtualEthernetSwitchPortSetting setting, IVirtualEthernetSwitchPort switchPort, IInternalEthernetPort internalPort, VMSwitch virtualSwitch)
        : base(setting, switchPort, virtualSwitch)
    {
        m_InternalNetworkAdapter = new DependentObjectDataUpdater<IInternalEthernetPort>(internalPort, (TimeSpan span) => (IInternalEthernetPort)switchPort.GetConnectedEthernetPort(span));
    }
}

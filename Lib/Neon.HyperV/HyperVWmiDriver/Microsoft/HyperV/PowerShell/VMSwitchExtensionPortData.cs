using System;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMSwitchExtensionPortData : VMSwitchExtensionRuntimeData
{
    private readonly VMNetworkAdapterBase m_ParentAdapter;

    public Guid VMId => m_ParentAdapter.VMId;

    public string VMName => m_ParentAdapter.VMName;

    public string VMNetworkAdapterName => m_ParentAdapter.Name;

    internal VMSwitchExtensionPortData(IEthernetPortStatus portStatus, VMNetworkAdapterBase parentAdapter)
        : base(portStatus)
    {
        m_ParentAdapter = parentAdapter;
    }
}

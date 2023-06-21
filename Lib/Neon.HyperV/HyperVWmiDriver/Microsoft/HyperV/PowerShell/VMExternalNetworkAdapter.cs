using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMExternalNetworkAdapter : VMInternalOrExternalNetworkAdapter
{
    private readonly IDataUpdater<ICollection<IExternalNetworkPort>> m_ExternalNetworkAdapters;

    public override bool IsExternalAdapter => true;

    public override string AdapterId => PrimaryExternalAdapter?.Name;

    internal string ExternalAdapterMacAddress => PrimaryExternalAdapter?.PermanentAddress;

    internal string InterfaceDescription => PrimaryExternalAdapter?.FriendlyName;

    internal IReadOnlyList<Guid> InterfaceIds
    {
        get
        {
            List<Guid> list = new List<Guid>();
            ICollection<IExternalNetworkPort> data = m_ExternalNetworkAdapters.GetData(UpdatePolicy.EnsureUpdated);
            if (data != null)
            {
                foreach (IExternalNetworkPort item in data)
                {
                    string text = item.DeviceId?.Replace("Microsoft:", string.Empty);
                    if (text != null && Guid.TryParse(text, out var result))
                    {
                        list.Add(result);
                    }
                }
            }
            return list.AsReadOnly();
        }
    }

    internal bool IovSupport => (IovCapabilities?.SupportsIov).GetValueOrDefault();

    internal IReadOnlyList<string> IovSupportReasons => IovCapabilities?.IovSupportReasons;

    private IExternalNetworkPort PrimaryExternalAdapter => m_ExternalNetworkAdapters.GetData(UpdatePolicy.EnsureUpdated)?.FirstOrDefault();

    private IExternalEthernetPortCapabilities IovCapabilities
    {
        get
        {
            IExternalNetworkPort primaryExternalAdapter = PrimaryExternalAdapter;
            primaryExternalAdapter?.UpdateAssociationCache(Constants.UpdateThreshold);
            return primaryExternalAdapter?.GetCapabilities();
        }
    }

    internal VMExternalNetworkAdapter(IVirtualEthernetSwitchPortSetting setting, IVirtualEthernetSwitchPort switchPort, IExternalNetworkPort[] connectedExternalPorts, VMSwitch virtualSwitch)
        : base(setting, switchPort, virtualSwitch)
    {
        m_ExternalNetworkAdapters = new CollectionDataUpdater<IExternalNetworkPort>(base.Server, connectedExternalPorts, GetConnectedExternalAdapters);
    }

    internal void SetConnectedExternalAdapters(IExternalNetworkPort[] externalAdapters)
    {
        m_ConnectionSetting.GetData(UpdatePolicy.None).HostResources = externalAdapters.Select((IExternalNetworkPort adapter) => adapter.ManagementPath).ToArray();
    }

    internal IExternalNetworkPort[] GetConnectedExternalAdapters()
    {
        return m_ConnectionSetting.GetData(UpdatePolicy.EnsureUpdated).HostResources.Select((WmiObjectPath hostResource) => (IExternalNetworkPort)ObjectLocator.GetVirtualizationManagementObject(base.Server, hostResource)).ToArray();
    }
}

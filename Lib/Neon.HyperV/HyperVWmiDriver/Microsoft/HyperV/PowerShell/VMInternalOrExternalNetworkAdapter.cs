using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.HyperV.PowerShell.Common;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal abstract class VMInternalOrExternalNetworkAdapter : VMNetworkAdapterBase
{
    protected readonly IDataUpdater<IVirtualEthernetSwitchPort> m_SwitchPort;

    protected readonly VMSwitch m_ParentSwitch;

    private readonly IEthernetSwitchFeatureService m_FeatureService;

    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "To hide this parameter per spec.")]
    private new string VMName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "To hide this parameter per spec.")]
    private new string VMId { get; set; }

    internal override string PutDescription => TaskDescriptions.SetVMNetworkAdapter_InternalOrExternal;

    public override string Name
    {
        get
        {
            return m_ConnectionSetting.GetData(UpdatePolicy.EnsureUpdated)?.FriendlyName;
        }
        internal set
        {
            IEthernetPortAllocationSettingData data = m_ConnectionSetting.GetData(UpdatePolicy.None);
            if (data != null)
            {
                data.FriendlyName = value;
            }
        }
    }

    public override string SwitchName => m_ParentSwitch.Name;

    internal IVirtualEthernetSwitchPort VirtualizationManagementPort => m_SwitchPort.GetData(UpdatePolicy.None);

    internal override IEthernetSwitchFeatureService FeatureService => m_FeatureService;

    internal VMInternalOrExternalNetworkAdapter(IVirtualEthernetSwitchPortSetting setting, IVirtualEthernetSwitchPort switchPort, VMSwitch virtualSwitch)
        : base(setting)
    {
        m_SwitchPort = new DataUpdater<IVirtualEthernetSwitchPort>(switchPort);
        m_ParentSwitch = virtualSwitch;
        m_FeatureService = ObjectLocator.GetVirtualSwitchManagementService(base.Server);
    }

    internal override IDataUpdater<IVMDeviceSetting> GetDeviceDataUpdater()
    {
        return m_ConnectionSetting;
    }

    internal override void PutSelf(IOperationWatcher operationWatcher)
    {
        operationWatcher.PerformPut(m_ConnectionSetting.GetData(UpdatePolicy.None), PutDescription, this);
    }

    internal override void RemoveSelf(IOperationWatcher operationWatcher)
    {
        IVirtualSwitchManagementService switchService = ObjectLocator.GetVirtualSwitchManagementService(base.Server);
        IVirtualEthernetSwitchPort switchPort = m_SwitchPort.GetData(UpdatePolicy.None);
        operationWatcher.PerformOperation(() => switchService.BeginRemoveVirtualSwitchPorts(new IVirtualEthernetSwitchPort[1] { switchPort }), switchService.EndRemoveVirtualSwitchPorts, TaskDescriptions.RemoveVMNetworkAdapter_FromSwitch, this);
        switchService.UpdateInternalEthernetPorts(TimeSpan.Zero);
        switchService.UpdateExternalNetworkPorts(TimeSpan.Zero);
    }

    internal override ILanEndpoint GetLanEndpoint()
    {
        return m_SwitchPort.GetData(UpdatePolicy.EnsureAssociatorsUpdated).LanEndpoint;
    }

    internal override VMSwitch GetConnectedSwitch()
    {
        return m_ParentSwitch;
    }

    internal override IVirtualEthernetSwitchPort GetSwitchPort()
    {
        return m_SwitchPort.GetData(UpdatePolicy.None);
    }

    internal static VMInternalOrExternalNetworkAdapter Create(IVirtualEthernetSwitchPort switchPort, VMSwitch virtualSwitch)
    {
        WmiObjectPath wmiObjectPath = null;
        IVirtualEthernetSwitchPortSetting virtualEthernetSwitchPortSetting = null;
        try
        {
            switchPort.UpdateAssociationCache();
            virtualEthernetSwitchPortSetting = switchPort.Setting;
            wmiObjectPath = virtualEthernetSwitchPortSetting.HostResource;
        }
        catch (ObjectNotFoundException)
        {
        }
        if (wmiObjectPath != null)
        {
            if (wmiObjectPath.ClassName.Equals("Msvm_ComputerSystem", StringComparison.OrdinalIgnoreCase))
            {
                return new VMInternalNetworkAdapter(virtualEthernetSwitchPortSetting, switchPort, (IInternalEthernetPort)switchPort.GetConnectedEthernetPort(Constants.UpdateThreshold), virtualSwitch);
            }
            return new VMExternalNetworkAdapter(virtualEthernetSwitchPortSetting, switchPort, virtualEthernetSwitchPortSetting.HostResources.Select((WmiObjectPath hostResource) => (IExternalNetworkPort)ObjectLocator.GetVirtualizationManagementObject(switchPort.Server, hostResource)).ToArray(), virtualSwitch);
        }
        return null;
    }
}

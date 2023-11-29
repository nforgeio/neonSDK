using System;
using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal sealed class VirtualEthernetSwitchView : VirtualSwitchView, IVirtualEthernetSwitch, IVirtualSwitch, IVirtualizationManagementObject, IPutable
{
    private const string gm_DefaultSwitchId = "c08cb7b8-9b3c-408e-8e30-5e16a3aeb444";

    public IVirtualEthernetSwitchSetting Setting => GetRelatedObject<IVirtualEthernetSwitchSetting>(base.Associations.SettingsDefineState);

    public IEnumerable<IVirtualEthernetSwitchPort> SwitchPorts => GetRelatedObjects<IVirtualEthernetSwitchPort>(base.Associations.EthernetSwitchToSwitchPort);

    public bool IsDefaultSwitch => string.Equals(base.InstanceId, "c08cb7b8-9b3c-408e-8e30-5e16a3aeb444", StringComparison.OrdinalIgnoreCase);

    public IEthernetSwitchOffloadStatus OffloadStatus => GetRelatedObject<IEthernetSwitchOffloadStatus>(base.Associations.EthernetSwitchToSwitchRuntimeStatus);

    public IEthernetSwitchBandwidthStatus BandwidthStatus => GetRelatedObject<IEthernetSwitchBandwidthStatus>(base.Associations.EthernetSwitchToSwitchRuntimeStatus);

    public IEnumerable<IEthernetSwitchStatus> GetRuntimeStatuses()
    {
        return GetRelatedObjects<IEthernetSwitchStatus>(base.Associations.EthernetSwitchToSwitchRuntimeStatus);
    }
}

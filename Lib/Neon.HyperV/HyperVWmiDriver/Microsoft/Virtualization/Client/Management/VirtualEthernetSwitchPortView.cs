using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal class VirtualEthernetSwitchPortView : EthernetPortView, IVirtualEthernetSwitchPort, IEthernetPort, IVirtualSwitchPort, IVirtualizationManagementObject
{
    public IVirtualEthernetSwitch VirtualEthernetSwitch => GetRelatedObject<IVirtualEthernetSwitch>(base.Associations.EthernetSwitchPortToSwitch);

    public IVirtualEthernetSwitchPortSetting Setting => GetRelatedObject<IVirtualEthernetSwitchPortSetting>(base.Associations.ElementSettingData);

    public IEthernetSwitchPortOffloadStatus OffloadStatus => GetRelatedObject<IEthernetSwitchPortOffloadStatus>(base.Associations.EthernetSwitchPortToPortRuntimeStatus);

    public IEthernetSwitchPortBandwidthStatus BandwidthStatus => GetRelatedObject<IEthernetSwitchPortBandwidthStatus>(base.Associations.EthernetSwitchPortToPortRuntimeStatus);

    public IEnumerable<IEthernetPortStatus> GetRuntimeStatuses()
    {
        return GetRelatedObjects<IEthernetPortStatus>(base.Associations.EthernetSwitchPortToPortRuntimeStatus);
    }
}

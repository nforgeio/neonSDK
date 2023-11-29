using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal class VirtualEthernetSwitchPortSettingView : EthernetPortAllocationSettingDataView, IVirtualEthernetSwitchPortSetting, IEthernetPortAllocationSettingData, IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable
{
    public IVirtualEthernetSwitchPort VirtualSwitchPort => GetRelatedObject<IVirtualEthernetSwitchPort>(base.Associations.ElementSettingData);

    protected override IVMTask BeginPutInternal(IDictionary<string, object> properties)
    {
        IVMTask iVMTask = ObjectLocator.GetVirtualSwitchManagementService(base.Server).BeginModifyVirtualSwitchPorts(new IVirtualEthernetSwitchPortSetting[1] { this });
        iVMTask.PutProperties = properties;
        return iVMTask;
    }
}

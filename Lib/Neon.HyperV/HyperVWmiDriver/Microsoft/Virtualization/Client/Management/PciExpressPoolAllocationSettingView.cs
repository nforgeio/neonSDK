using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Virtualization.Client.Management;

internal class PciExpressPoolAllocationSettingView : ResourcePoolAllocationSettingView, IPciExpressPoolAllocationSetting, IResourcePoolAllocationSetting, IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable
{
    protected WmiObjectPath[] PciExpressDevices
    {
        get
        {
            return WmiObjectPath.FromStringArray(GetProperty<string[]>("HostResource") ?? new string[0]);
        }
        set
        {
            SetProperty("HostResource", WmiObjectPath.ToStringArray(value));
        }
    }

    public IEnumerable<IVMAssignableDevice> GetPciExpressDevices()
    {
        return PciExpressDevices.Select((WmiObjectPath hostResource) => (IVMAssignableDevice)ObjectLocator.GetVirtualizationManagementObject(base.Server, hostResource));
    }

    public void SetPciExpressDevices(IList<IVMAssignableDevice> pciExpressDevices)
    {
        PciExpressDevices = ((pciExpressDevices == null) ? new WmiObjectPath[0] : pciExpressDevices.Select((IVMAssignableDevice s) => s.ManagementPath).ToArray());
    }
}

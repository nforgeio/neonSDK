using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Virtualization.Client.Management;

internal class GsmPoolAllocationSettingView : ResourcePoolAllocationSettingView, IGsmPoolAllocationSetting, IResourcePoolAllocationSetting, IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable
{
    protected WmiObjectPath[] Switches
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

    public bool HasAnySwitch()
    {
        string[] property = GetProperty<string[]>("HostResource");
        if (property != null)
        {
            return property.Length != 0;
        }
        return false;
    }

    public bool HasSwitch(IVirtualSwitch virtualSwitch)
    {
        if (virtualSwitch == null)
        {
            return false;
        }
        return Switches.Contains(virtualSwitch.ManagementPath);
    }

    public IEnumerable<IVirtualSwitch> GetSwitches()
    {
        return Switches.Select((WmiObjectPath hostResource) => (IVirtualSwitch)ObjectLocator.GetVirtualizationManagementObject(base.Server, hostResource));
    }

    public void RemoveSwitch(IVirtualSwitch virtualSwitch)
    {
        if (virtualSwitch != null)
        {
            Switches = Switches.Where((WmiObjectPath hostResource) => hostResource != virtualSwitch.ManagementPath).ToArray();
        }
    }

    public void AddSwitch(IVirtualSwitch virtualSwitch)
    {
        if (virtualSwitch != null)
        {
            WmiObjectPath[] switches = Switches;
            if (!switches.Contains(virtualSwitch.ManagementPath))
            {
                WmiObjectPath[] array = new WmiObjectPath[switches.Length + 1];
                Array.Copy(switches, array, switches.Length);
                array[switches.Length] = virtualSwitch.ManagementPath;
                Switches = array;
            }
        }
    }

    public void SetSwitches(IList<IVirtualSwitch> virtualSwitches)
    {
        Switches = ((virtualSwitches == null) ? new WmiObjectPath[0] : virtualSwitches.Select((IVirtualSwitch s) => s.ManagementPath).ToArray());
    }
}

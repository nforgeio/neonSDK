using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Virtualization.Client.Management;

internal class EthernetConnectionResourcePoolView : ResourcePoolView, IEthernetConnectionResourcePool, IResourcePool, IVirtualizationManagementObject, IDeleteableAsync, IDeleteable, IMetricMeasurableElement
{
    public IEnumerable<IVirtualEthernetSwitch> GetSwitches()
    {
        if (base.Primordial)
        {
            return GetRelatedObjects<IVirtualEthernetSwitch>(base.Associations.ResourcePoolSystemComponents);
        }
        return base.AllocationSettings.Cast<IGsmPoolAllocationSetting>().SelectMany((IGsmPoolAllocationSetting setting) => setting.GetSwitches()).Cast<IVirtualEthernetSwitch>();
    }

    public bool HasSwitch(IVirtualEthernetSwitch virtualSwitch)
    {
        return (from s in GetSwitches()
            select s.ManagementPath).Contains(virtualSwitch.ManagementPath);
    }
}

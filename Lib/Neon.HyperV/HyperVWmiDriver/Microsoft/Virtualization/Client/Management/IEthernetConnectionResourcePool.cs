using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_ResourcePool", PrimaryMapping = false)]
internal interface IEthernetConnectionResourcePool : IResourcePool, IVirtualizationManagementObject, IDeleteableAsync, IDeleteable, IMetricMeasurableElement
{
    IEnumerable<IVirtualEthernetSwitch> GetSwitches();

    bool HasSwitch(IVirtualEthernetSwitch virtualSwitch);
}

using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_ResourcePool", PrimaryMapping = false)]
internal interface IFibreChannelResourcePool : IResourcePool, IVirtualizationManagementObject, IDeleteableAsync, IDeleteable, IMetricMeasurableElement
{
	IFcPoolAllocationSetting FcPoolConnectionAllocationSetting { get; }

	IEnumerable<IVirtualFcSwitch> GetVirtualFcSwitches(bool updateRasdPropertyCache);

	bool HasSwitch(IVirtualFcSwitch virtualSwitch);
}

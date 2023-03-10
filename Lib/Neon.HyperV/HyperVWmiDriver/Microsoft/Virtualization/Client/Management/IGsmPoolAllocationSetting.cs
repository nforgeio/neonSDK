using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal interface IGsmPoolAllocationSetting : IResourcePoolAllocationSetting, IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable
{
	bool HasAnySwitch();

	bool HasSwitch(IVirtualSwitch virtualSwitch);

	IEnumerable<IVirtualSwitch> GetSwitches();

	void RemoveSwitch(IVirtualSwitch virtualSwitch);

	void AddSwitch(IVirtualSwitch virtualSwitch);

	void SetSwitches(IList<IVirtualSwitch> virtualSwitches);
}

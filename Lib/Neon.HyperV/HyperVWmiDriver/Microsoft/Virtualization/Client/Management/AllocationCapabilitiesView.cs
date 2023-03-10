using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal class AllocationCapabilitiesView : View, IAllocationCapabilities, IVirtualizationManagementObject
{
	public IEnumerable<IVMDeviceSetting> Capabilities => GetRelatedObjects<IVMDeviceSetting>(base.Associations.SettingsDefineCapabilities);
}

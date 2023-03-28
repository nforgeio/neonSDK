using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_AllocationCapabilities")]
internal interface IAllocationCapabilities : IVirtualizationManagementObject
{
	IEnumerable<IVMDeviceSetting> Capabilities { get; }
}

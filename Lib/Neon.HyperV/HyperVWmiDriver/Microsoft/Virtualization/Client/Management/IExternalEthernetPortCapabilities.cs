using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_ExternalEthernetPortCapabilities")]
internal interface IExternalEthernetPortCapabilities : IVirtualizationManagementObject
{
	bool SupportsIov { get; }

	IReadOnlyList<string> IovSupportReasons { get; }
}

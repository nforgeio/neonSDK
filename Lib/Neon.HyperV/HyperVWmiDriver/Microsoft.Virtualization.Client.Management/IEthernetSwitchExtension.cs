using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_EthernetSwitchExtension")]
internal interface IEthernetSwitchExtension : IPutableAsync, IPutable, IEthernetSwitchExtensionBase, IVirtualizationManagementObject
{
	bool IsChild { get; }

	bool IsEnabled { get; set; }

	bool IsRunning { get; }

	IEnumerable<IEthernetSwitchExtension> Children { get; }

	IVirtualEthernetSwitch Switch { get; }

	IEthernetSwitchExtension Parent { get; }
}

using System;
using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_EthernetSwitchPortRoutingDomainSettingData")]
internal interface IEthernetSwitchPortRoutingDomainFeature : IEthernetSwitchPortFeature, IEthernetFeature, IVirtualizationManagementObject
{
	Guid RoutingDomainId { get; set; }

	string RoutingDomainName { get; set; }

	IReadOnlyCollection<int> IsolationIds { get; set; }

	IReadOnlyCollection<string> IsolationNames { get; set; }
}

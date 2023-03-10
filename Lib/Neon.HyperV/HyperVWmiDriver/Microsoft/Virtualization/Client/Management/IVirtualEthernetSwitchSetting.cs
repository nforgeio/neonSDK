using System;
using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_VirtualEthernetSwitchSettingData")]
internal interface IVirtualEthernetSwitchSetting : IVirtualizationManagementObject, IPutableAsync, IPutable
{
	[Key]
	string InstanceId { get; }

	Guid Id { get; }

	string SwitchFriendlyName { get; set; }

	string Notes { get; set; }

	bool IOVPreferred { get; set; }

	IList<IEthernetSwitchExtension> ExtensionList { get; set; }

	IVirtualEthernetSwitch Switch { get; }

	BandwidthReservationMode BandwidthReservationMode { get; }

	bool PacketDirectEnabled { get; }

	bool TeamingEnabled { get; }

	IEnumerable<IEthernetSwitchFeature> Features { get; }
}

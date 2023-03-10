using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_FailoverNetworkAdapterSettingData")]
internal interface IFailoverNetworkAdapterSetting : IVirtualizationManagementObject, IPutableAsync, IPutable
{
	VMNetworkAdapterProtocolType ProtocolIFType { get; set; }

	bool DhcpEnabled { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
	string[] IPAddresses { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
	string[] Subnets { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
	string[] DefaultGateways { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
	string[] DnsServers { get; set; }
}

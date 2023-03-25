using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_VirtualEthernetSwitchManagementCapabilities")]
internal interface IVirtualEthernetSwitchManagementCapabilities : IVirtualizationManagementObject
{
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "IOV", Justification = "This is by spec.")]
	bool IOVSupport { get; }

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "IOV", Justification = "This is by spec.")]
	string[] IOVSupportReasons { get; }
}

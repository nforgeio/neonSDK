using System.Diagnostics.CodeAnalysis;

namespace Microsoft.HyperV.PowerShell;

[SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue", Justification = "This is consistent with MOF.")]
[SuppressMessage("Microsoft.Design", "CA1028:EnumStorageShouldBeInt32", Justification = "This is consistent with MOF.")]
internal enum VMNetworkAdapterAclType : byte
{
	Mac = 1,
	[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Pv", Justification = "This is per spec.")]
	IPv4,
	[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Pv", Justification = "This is per spec.")]
	IPv6,
	[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Pv", Justification = "This is per spec.")]
	WildcardIPv4,
	[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Pv", Justification = "This is per spec.")]
	WildcardIPv6,
	WildcardBoth,
	WildcardMac
}

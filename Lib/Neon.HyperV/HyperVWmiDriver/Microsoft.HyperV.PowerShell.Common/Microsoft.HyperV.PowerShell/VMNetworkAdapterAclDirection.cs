using System.Diagnostics.CodeAnalysis;

namespace Microsoft.HyperV.PowerShell;

[SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue", Justification = "This is consistent with MOF.")]
[SuppressMessage("Microsoft.Design", "CA1028:EnumStorageShouldBeInt32", Justification = "This is consistent with MOF.")]
internal enum VMNetworkAdapterAclDirection : byte
{
	Inbound = 1,
	Outbound,
	Both
}

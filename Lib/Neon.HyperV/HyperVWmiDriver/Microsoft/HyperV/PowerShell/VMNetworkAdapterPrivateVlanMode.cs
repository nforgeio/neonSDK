using System.Diagnostics.CodeAnalysis;

namespace Microsoft.HyperV.PowerShell;

[SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue", Justification = "This is consistent with the MOF definition.")]
internal enum VMNetworkAdapterPrivateVlanMode
{
	Isolated = 1,
	Community,
	Promiscuous
}

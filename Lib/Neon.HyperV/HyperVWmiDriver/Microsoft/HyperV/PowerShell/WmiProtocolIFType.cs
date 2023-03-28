using System.Diagnostics.CodeAnalysis;

namespace Microsoft.HyperV.PowerShell;

internal enum WmiProtocolIFType
{
	None = 0,
	Other = 1,
	[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Pv", Justification = "This is by spec.")]
	IPv4 = 4096,
	[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Pv", Justification = "This is by spec.")]
	IPv6 = 4097,
	[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "v", Justification = "This is by spec.")]
	[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Pv", Justification = "This is by spec.")]
	IPv4v6 = 4098
}

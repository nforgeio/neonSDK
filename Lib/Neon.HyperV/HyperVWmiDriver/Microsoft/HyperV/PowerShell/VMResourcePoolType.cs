using System.Diagnostics.CodeAnalysis;

namespace Microsoft.HyperV.PowerShell;

internal enum VMResourcePoolType
{
    Memory,
    Processor,
    Ethernet,
    [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "VHD", Justification = "This is by spec.")]
    VHD,
    [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "ISO", Justification = "This is by spec.")]
    ISO,
    [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "VFD", Justification = "This is by spec.")]
    VFD,
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Fibre", Justification = "This is per spec.")]
    FibreChannelPort,
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Fibre", Justification = "This is per spec.")]
    FibreChannelConnection,
    PciExpress
}

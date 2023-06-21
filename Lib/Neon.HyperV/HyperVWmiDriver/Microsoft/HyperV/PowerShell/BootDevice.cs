using System.Diagnostics.CodeAnalysis;

namespace Microsoft.HyperV.PowerShell;

internal enum BootDevice
{
    Floppy,
    CD,
    [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "IDE", Justification = "This is per spec.")]
    IDE,
    LegacyNetworkAdapter,
    NetworkAdapter,
    VHD
}

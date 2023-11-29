using System.Diagnostics.CodeAnalysis;

namespace Microsoft.HyperV.PowerShell;

[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Iov", Justification = "This is by spec.")]
internal enum IovInterruptModerationValue
{
    Default = 0,
    Adaptive = 1,
    Off = 2,
    Low = 100,
    Medium = 200,
    High = 300
}

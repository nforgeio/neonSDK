using System;

namespace Microsoft.HyperV.PowerShell;

[Flags]
internal enum IdentifierFlags
{
    None = 0,
    UniqueIdentifier = 1,
    FriendlyName = 2
}

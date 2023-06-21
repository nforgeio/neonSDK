using System;

namespace Microsoft.Virtualization.Client;

[Flags]
internal enum VMTraceTagFormatLevels
{
    None = 0,
    Timestamp = 1,
    SourceInformation = 2
}

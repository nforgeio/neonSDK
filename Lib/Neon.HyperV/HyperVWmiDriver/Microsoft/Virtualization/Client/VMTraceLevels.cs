using System;

namespace Microsoft.Virtualization.Client;

[Flags]
internal enum VMTraceLevels
{
    None = 0,
    Error = 1,
    Warning = 2,
    UserActions = 4,
    Information = 8,
    WmiCalls = 0x10,
    WmiEvents = 0x20,
    Verbose = 0x40,
    VerboseWmiGetProperties = 0x80,
    VerboseWmiEventProperties = 0x100
}

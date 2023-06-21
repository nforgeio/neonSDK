using System.ComponentModel;

namespace Microsoft.HyperV.PowerShell;

[TypeConverter(typeof(VMTaskStatusEnumResourceConverter))]
internal enum TaskStatus
{
    Deleted = 0,
    New = 2,
    Starting = 3,
    Running = 4,
    Suspended = 5,
    ShuttingDown = 6,
    Completed = 7,
    Terminated = 8,
    Killed = 9,
    Exception = 10,
    CompletedWithWarnings = 32768
}

using System.Management.Automation;

namespace Microsoft.HyperV.PowerShell.Commands;

internal interface ISupportsPassthrough
{
    SwitchParameter Passthru { get; set; }
}

using System.Management.Automation;

namespace Microsoft.HyperV.PowerShell.Commands;

internal interface ISupportsForce
{
    SwitchParameter Force { get; set; }
}

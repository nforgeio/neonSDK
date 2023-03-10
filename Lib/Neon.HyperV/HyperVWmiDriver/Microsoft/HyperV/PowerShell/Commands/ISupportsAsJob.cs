using System.Management.Automation;

namespace Microsoft.HyperV.PowerShell.Commands;

internal interface ISupportsAsJob
{
	SwitchParameter AsJob { get; set; }
}

using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;

namespace Microsoft.HyperV.PowerShell.Commands;

[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Eventing", Justification = "This is by spec.")]
[Cmdlet("Disable", "VMEventing", SupportsShouldProcess = true)]
internal sealed class DisableVMEventing : VirtualizationCmdletBase, ISupportsForce
{
	[Parameter]
	public SwitchParameter Force { get; set; }

	internal override void PerformOperation(IOperationWatcher operationWatcher)
	{
		if (operationWatcher.ShouldProcess(CmdletResources.ShouldProcess_DisableVMEventing))
		{
			operationWatcher.ShouldContinue(CmdletResources.ShouldContinue_DisableVMEventing);
		}
	}
}

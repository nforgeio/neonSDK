using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell.Commands;

[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Eventing", Justification = "This is by spec.")]
[Cmdlet("Enable", "VMEventing", SupportsShouldProcess = true)]
internal sealed class EnableVMEventing : VirtualizationCmdletBase, ISupportsForce
{
    [Parameter]
    public SwitchParameter Force { get; set; }

    internal override void PerformOperation(IOperationWatcher operationWatcher)
    {
        if (!operationWatcher.ShouldProcess(CmdletResources.ShouldProcess_EnableVMEventing) || !operationWatcher.ShouldContinue(CmdletResources.ShouldContinue_EnableVMEventing))
        {
            return;
        }
        foreach (Server server in ParameterResolvers.GetServers(this, operationWatcher))
        {
            server.FlushCache();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Enable", "VMMigration", SupportsShouldProcess = true, DefaultParameterSetName = "ComputerName")]
[OutputType(new Type[] { typeof(VMHost) })]
internal sealed class EnableVMMigration : VirtualizationCmdlet<VMHost>, ISupportsPassthrough
{
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
    [Parameter]
    public SwitchParameter Passthru { get; set; }

    [Parameter(ParameterSetName = "CimSession", Position = 0, Mandatory = true)]
    [ValidateNotNullOrEmpty]
    public override CimSession[] CimSession { get; set; }

    [Parameter(ParameterSetName = "ComputerName", Position = 0)]
    [ValidateNotNullOrEmpty]
    public override string[] ComputerName { get; set; }

    [Parameter(ParameterSetName = "ComputerName", Position = 1)]
    [ValidateNotNullOrEmpty]
    [CredentialArray]
    public override PSCredential[] Credential { get; set; }

    internal override IList<VMHost> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        return VirtualizationObjectLocator.GetVMHosts(ParameterResolvers.GetServers(this, operationWatcher), operationWatcher);
    }

    internal override void ProcessOneOperand(VMHost operand, IOperationWatcher operationWatcher)
    {
        if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_EnableVMMigration, operand.Server)))
        {
            operand.VirtualMachineMigrationEnabled = true;
            ((IUpdatable)operand).Put(operationWatcher);
            if ((bool)Passthru)
            {
                operationWatcher.WriteObject(operand);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Get", "VMHost", DefaultParameterSetName = "ComputerName")]
[OutputType(new Type[] { typeof(VMHost) })]
internal sealed class GetVMHost : VirtualizationCmdlet<VMHost>
{
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, Mandatory = true, ParameterSetName = "CimSession")]
    [ValidateNotNullOrEmpty]
    public override CimSession[] CimSession { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ValueFromPipeline = true, Position = 0, ParameterSetName = "ComputerName")]
    [ValidateNotNullOrEmpty]
    public override string[] ComputerName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ValueFromPipeline = true, Position = 1, ParameterSetName = "ComputerName")]
    [ValidateNotNullOrEmpty]
    [CredentialArray]
    public override PSCredential[] Credential { get; set; }

    internal override IList<VMHost> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        return VirtualizationObjectLocator.GetVMHosts(ParameterResolvers.GetServers(this, operationWatcher), operationWatcher);
    }

    internal override void ProcessOneOperand(VMHost operand, IOperationWatcher operationWatcher)
    {
        operationWatcher.WriteObject(operand);
    }
}

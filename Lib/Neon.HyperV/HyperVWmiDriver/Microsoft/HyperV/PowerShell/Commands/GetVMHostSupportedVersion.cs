using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management.Automation;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Get", "VMHostSupportedVersion", DefaultParameterSetName = "ComputerName")]
[OutputType(new Type[] { typeof(VMHostSupportedVersion) })]
internal sealed class GetVMHostSupportedVersion : VirtualizationCmdlet<VMHostSupportedVersion>
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
    [Parameter(Position = 1, ParameterSetName = "ComputerName")]
    [ValidateNotNullOrEmpty]
    [CredentialArray]
    public override PSCredential[] Credential { get; set; }

    [Parameter]
    public SwitchParameter Default { get; set; }

    internal override IList<VMHostSupportedVersion> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        IEnumerable<VMHostSupportedVersion> source = VMHostSupportedVersion.GetVmHostSupportedVersions(ParameterResolvers.GetServers(this, operationWatcher), operationWatcher);
        if (Default.ToBool())
        {
            source = source.Where((VMHostSupportedVersion version) => version.IsDefault);
        }
        return source.ToList();
    }

    internal override void ProcessOneOperand(VMHostSupportedVersion operand, IOperationWatcher operationWatcher)
    {
        operationWatcher.WriteObject(operand);
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Get", "VMRemoteFXPhysicalVideoAdapter", DefaultParameterSetName = "ComputerName")]
[OutputType(new Type[] { typeof(VMRemoteFXPhysicalVideoAdapter) })]
internal sealed class GetVMRemoteFXPhysicalVideoAdapter : VirtualizationCmdlet<VMRemoteFXPhysicalVideoAdapter>
{
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = "CimSession")]
    [ValidateNotNullOrEmpty]
    public override CimSession[] CimSession { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = "ComputerName")]
    [ValidateNotNullOrEmpty]
    public override string[] ComputerName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "ComputerName")]
    [ValidateNotNullOrEmpty]
    [CredentialArray]
    public override PSCredential[] Credential { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter(ValueFromPipeline = true, Position = 0)]
    public string[] Name { get; set; }

    internal override IList<VMRemoteFXPhysicalVideoAdapter> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        WriteWarning(string.Format(CultureInfo.CurrentCulture, CmdletResources.RemoteFX_CmdletWarning, "warning"));
        return VMRemoteFXPhysicalVideoAdapter.GetVmRemoteFxPhysicalVideoAdapters(ParameterResolvers.GetServers(this, operationWatcher), Name).ToList();
    }

    internal override void ProcessOneOperand(VMRemoteFXPhysicalVideoAdapter operand, IOperationWatcher operationWatcher)
    {
        operationWatcher.WriteObject(operand);
    }
}

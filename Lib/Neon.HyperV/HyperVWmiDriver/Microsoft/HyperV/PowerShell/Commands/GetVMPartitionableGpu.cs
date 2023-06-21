using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management.Automation;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Get", "VMPartitionableGpu", DefaultParameterSetName = "ComputerName")]
[OutputType(new Type[] { typeof(VMPartitionableGpu) })]
internal sealed class GetVMPartitionableGpu : VirtualizationCmdlet<VMPartitionableGpu>
{
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "CimSession")]
    [ValidateNotNullOrEmpty]
    public override CimSession[] CimSession { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(Position = 0, ParameterSetName = "ComputerName")]
    [ValidateNotNullOrEmpty]
    public override string[] ComputerName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(Position = 1, ParameterSetName = "ComputerName")]
    [ValidateNotNullOrEmpty]
    [CredentialArray]
    public override PSCredential[] Credential { get; set; }

    [ValidateNotNullOrEmpty]
    [Parameter]
    public string Name { get; set; }

    internal override IList<VMPartitionableGpu> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        List<VMPartitionableGpu> list = ParameterResolvers.GetServers(this, operationWatcher).SelectMany(VMPartitionableGpu.GetVMPartitionableGpus).ToList();
        if (!string.IsNullOrEmpty(Name))
        {
            list = list.Where((VMPartitionableGpu pGpu) => string.Equals(Name, pGpu.Name, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        return list;
    }

    internal override void ProcessOneOperand(VMPartitionableGpu operand, IOperationWatcher operationWatcher)
    {
        operationWatcher.WriteObject(operand);
    }
}

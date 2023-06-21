using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management.Automation;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Get", "VMHostNumaNode", DefaultParameterSetName = "ComputerName")]
[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Numa", Justification = "This is by spec.")]
[OutputType(new Type[] { typeof(VMHostNumaNode) })]
internal sealed class GetVMHostNumaNode : VirtualizationCmdlet<VMHostNumaNode>
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

    [ValidateNotNull]
    [Parameter]
    public int? Id { get; set; }

    internal override IList<VMHostNumaNode> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        List<VMHostNumaNode> list = ParameterResolvers.GetServers(this, operationWatcher).SelectMany(VMHostNumaNode.GetHostNumaNodes).ToList();
        if (Id.HasValue)
        {
            list = list.Where((VMHostNumaNode node) => Id.Value == node.NodeId).ToList();
        }
        return list;
    }

    internal override void ProcessOneOperand(VMHostNumaNode operand, IOperationWatcher operationWatcher)
    {
        operationWatcher.WriteObject(operand);
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Set", "VMPartitionableGpu", DefaultParameterSetName = "ComputerName")]
[OutputType(new Type[] { typeof(VMPartitionableGpu) })]
internal sealed class SetVMPartitionableGpu : VirtualizationCmdlet<VMPartitionableGpu>, ISupportsPassthrough
{
    [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "CimSession")]
    [ValidateNotNullOrEmpty]
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    public override CimSession[] CimSession { get; set; }

    [Parameter(Position = 0, ParameterSetName = "ComputerName")]
    [ValidateNotNullOrEmpty]
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    public override string[] ComputerName { get; set; }

    [Parameter(Position = 1, ParameterSetName = "ComputerName")]
    [ValidateNotNullOrEmpty]
    [CredentialArray]
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    public override PSCredential[] Credential { get; set; }

    [Parameter(ParameterSetName = "Object", ValueFromPipeline = true, Position = 0, Mandatory = true)]
    [ValidateNotNullOrEmpty]
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    public VMPartitionableGpu[] PartitionableGpu { get; set; }

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
    [Parameter]
    public SwitchParameter Passthru { get; set; }

    [Parameter(ParameterSetName = "Name")]
    [ValidateNotNullOrEmpty]
    public string Name { get; set; }

    [Parameter]
    [ValidateNotNullOrEmpty]
    public ushort? PartitionCount { get; set; }

    internal override IList<VMPartitionableGpu> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        IList<VMPartitionableGpu> list;
        if (CurrentParameterSetIs("Object"))
        {
            list = PartitionableGpu;
        }
        else
        {
            list = ParameterResolvers.GetServers(this, operationWatcher).SelectMany(VMPartitionableGpu.GetVMPartitionableGpus).ToList();
            if (!string.IsNullOrEmpty(Name))
            {
                list = list.Where((VMPartitionableGpu pGpu) => string.Equals(Name, pGpu.Name, StringComparison.OrdinalIgnoreCase)).ToList();
            }
        }
        return list;
    }

    internal override void ProcessOneOperand(VMPartitionableGpu operand, IOperationWatcher operationWatcher)
    {
        if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_SetVMPartitionableGpu, operand.Name)) && PartitionCount.HasValue)
        {
            operand.PartitionCount = PartitionCount.Value;
            ((IUpdatable)operand).Put(operationWatcher);
        }
        if ((bool)Passthru)
        {
            operationWatcher.WriteObject(operand);
        }
    }
}

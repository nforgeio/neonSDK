using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Get", "VMGpuPartitionAdapter", DefaultParameterSetName = "VMName")]
[OutputType(new Type[] { typeof(VMGpuPartitionAdapter) })]
internal sealed class GetVMGpuPartitionAdapter : VirtualizationCmdlet<VMGpuPartitionAdapter>, IVMObjectOrVMNameCmdlet, IVmByObjectCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmByVMNameCmdlet, IVMSnapshotCmdlet
{
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "VMName")]
    [ValidateNotNullOrEmpty]
    public override CimSession[] CimSession { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "VMName")]
    [ValidateNotNullOrEmpty]
    public override string[] ComputerName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "VMName")]
    [ValidateNotNullOrEmpty]
    [CredentialArray]
    public override PSCredential[] Credential { get; set; }

    [ValidateNotNullOrEmpty]
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "VMSnapshot")]
    [Alias(new string[] { "VMCheckpoint" })]
    public VMSnapshot VMSnapshot { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter(ParameterSetName = "VMName", ValueFromPipeline = true, Position = 0, Mandatory = true)]
    public string[] VMName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter(ParameterSetName = "VMObject", ValueFromPipeline = true, Position = 0, Mandatory = true)]
    public VirtualMachine[] VM { get; set; }

    [ValidateNotNullOrEmpty]
    [Parameter(ParameterSetName = "VMName")]
    [Parameter(ParameterSetName = "VMObject")]
    public string AdapterId { get; set; }

    internal override IList<VMGpuPartitionAdapter> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        IEnumerable<VirtualMachineBase> enumerable = null;
        enumerable = ((!CurrentParameterSetIs("VMSnapshot")) ? ((IEnumerable<VirtualMachineBase>)ParameterResolvers.ResolveVirtualMachines(this, operationWatcher)) : ((IEnumerable<VirtualMachineBase>)new VirtualMachineBase[1] { VMSnapshot }));
        return enumerable.SelectManyWithLogging((VirtualMachineBase vm) => vm.GetGpuPartitionAdapters(AdapterId), operationWatcher).ToList();
    }

    internal override void ProcessOneOperand(VMGpuPartitionAdapter operand, IOperationWatcher operationWatcher)
    {
        operationWatcher.WriteObject(operand);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Get", "VMIdeController", DefaultParameterSetName = "VMName")]
[OutputType(new Type[] { typeof(VMIdeController) })]
internal sealed class GetVMIdeController : VirtualizationCmdlet<VMIdeController>, IVMObjectOrVMNameCmdlet, IVmByObjectCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmByVMNameCmdlet, IVMSnapshotCmdlet
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

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter(ParameterSetName = "VMObject", ValueFromPipeline = true, Position = 0, Mandatory = true)]
    public VirtualMachine[] VM { get; set; }

    [ValidateNotNullOrEmpty]
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "VMSnapshot")]
    [Alias(new string[] { "VMCheckpoint" })]
    public VMSnapshot VMSnapshot { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter(ParameterSetName = "VMName", ValueFromPipeline = true, Position = 0, Mandatory = true)]
    public string[] VMName { get; set; }

    [ValidateNotNull]
    [ValidateRange(0, 1)]
    [Parameter(Position = 1)]
    public int? ControllerNumber { get; set; }

    internal override IList<VMIdeController> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        IEnumerable<VirtualMachineBase> inputs = (IEnumerable<VirtualMachineBase>)((!IsParameterSpecified("VMSnapshot")) ? ((IEnumerable)ParameterResolvers.ResolveVirtualMachines(this, operationWatcher)) : ((IEnumerable)new VMSnapshot[1] { VMSnapshot }));
        IEnumerable<VMIdeController> source = inputs.SelectManyWithLogging((VirtualMachineBase vm) => vm.GetIdeControllers(), operationWatcher);
        if (ControllerNumber.HasValue)
        {
            source = source.Where((VMIdeController controller) => controller.ControllerNumber == ControllerNumber.Value);
        }
        return source.ToList();
    }

    internal override void ProcessOneOperand(VMIdeController operand, IOperationWatcher operationWatcher)
    {
        operationWatcher.WriteObject(operand);
    }
}

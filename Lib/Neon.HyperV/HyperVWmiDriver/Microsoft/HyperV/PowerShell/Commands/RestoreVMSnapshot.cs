using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.HyperV.PowerShell.Common;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Restore", "VMSnapshot", DefaultParameterSetName = "SnapshotName", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
[OutputType(new Type[] { typeof(VMSnapshot) })]
internal sealed class RestoreVMSnapshot : VirtualizationCmdlet<VMSnapshot>, IVmBySingularObjectCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmBySingularVMNameCmdlet, IVMSnapshotCmdlet, ISupportsPassthrough, ISupportsAsJob
{
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "SnapshotName")]
    [ValidateNotNullOrEmpty]
    public override CimSession[] CimSession { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "SnapshotName")]
    [ValidateNotNullOrEmpty]
    public override string[] ComputerName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "SnapshotName")]
    [ValidateNotNullOrEmpty]
    [CredentialArray]
    public override PSCredential[] Credential { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec, plus array is easier to use for users.")]
    [ValidateNotNull]
    [Parameter(Mandatory = true, Position = 0, ParameterSetName = "SnapshotObject", ValueFromPipeline = true)]
    [Alias(new string[] { "VMCheckpoint" })]
    public VMSnapshot VMSnapshot { get; set; }

    [ValidateNotNull]
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "VM")]
    public VirtualMachine VM { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec, plus array is easier to use for users.")]
    [ValidateNotNullOrEmpty]
    [Parameter(Mandatory = true, ParameterSetName = "VM")]
    [Parameter(Mandatory = true, Position = 0, ParameterSetName = "SnapshotName")]
    public string Name { get; set; }

    [ValidateNotNullOrEmpty]
    [Parameter(Mandatory = true, Position = 1, ParameterSetName = "SnapshotName")]
    public string VMName { get; set; }

    [Parameter]
    public SwitchParameter AsJob { get; set; }

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
    [Parameter]
    public SwitchParameter Passthru { get; set; }

    internal override IList<VMSnapshot> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        if (CurrentParameterSetIs("SnapshotObject"))
        {
            return new VMSnapshot[1] { VMSnapshot };
        }
        IEnumerable<VMSnapshot> source = ParameterResolvers.ResolveVirtualMachines(this, operationWatcher).SelectMany((VirtualMachine vm) => vm.GetVMSnapshots());
        if (!string.IsNullOrEmpty(Name))
        {
            WildcardPattern pattern = new WildcardPattern(Name, WildcardOptions.Compiled | WildcardOptions.IgnoreCase);
            source = source.Where((VMSnapshot snapshot) => pattern.IsMatch(snapshot.Name));
        }
        return source.ToList();
    }

    internal override void ValidateOperandList(IList<VMSnapshot> operands, IOperationWatcher operationWatcher)
    {
        base.ValidateOperandList(operands, operationWatcher);
        int count = operands.Count;
        if (count == 0)
        {
            throw ExceptionHelper.CreateObjectNotFoundException(ErrorMessages.SnapshotNotFound, null);
        }
        if (count > 1)
        {
            throw ExceptionHelper.CreateInvalidArgumentException(ErrorMessages.MoreThanOneSnapshotFound);
        }
    }

    internal override void ProcessOneOperand(VMSnapshot operand, IOperationWatcher operationWatcher)
    {
        if (!operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_RestoreVMSnapshot, operand.Name)))
        {
            return;
        }
        VirtualMachine virtualMachine = operand.GetVirtualMachine();
        VMState currentState = virtualMachine.GetCurrentState();
        if (currentState == VMState.Running || currentState == VMState.Paused)
        {
            virtualMachine.ChangeState(VirtualMachineAction.Save, operationWatcher);
        }
        try
        {
            operand.Apply(operationWatcher);
            if (Passthru.IsPresent)
            {
                operationWatcher.WriteObject(operand);
            }
        }
        finally
        {
            if (virtualMachine.GetCurrentState() == VMState.Saved && currentState == VMState.Running)
            {
                virtualMachine.ChangeState(VirtualMachineAction.Start, operationWatcher);
            }
        }
    }
}

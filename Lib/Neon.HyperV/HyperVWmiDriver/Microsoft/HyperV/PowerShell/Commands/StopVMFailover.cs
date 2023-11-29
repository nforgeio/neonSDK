using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Stop", "VMFailover", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium, DefaultParameterSetName = "VMName")]
[OutputType(new Type[] { typeof(VirtualMachine) })]
internal sealed class StopVMFailover : VirtualizationCmdlet<VirtualMachine>, IVmByVMNameCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmByObjectCmdlet, ISupportsPassthrough, ISupportsAsJob
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

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter(ParameterSetName = "VMName", ValueFromPipeline = true, Position = 0, Mandatory = true)]
    public string[] VMName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter(ParameterSetName = "VMObject", ValueFromPipeline = true, Position = 0, Mandatory = true)]
    public VirtualMachine[] VM { get; set; }

    [Parameter]
    public SwitchParameter AsJob { get; set; }

    [Parameter]
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
    public SwitchParameter Passthru { get; set; }

    internal override IList<VirtualMachine> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        return ParameterResolvers.ResolveVirtualMachines(this, operationWatcher);
    }

    internal override void ProcessOneOperand(VirtualMachine operand, IOperationWatcher operationWatcher)
    {
        bool flag = false;
        VMReplication vMReplication = VMReplication.GetVMReplication(operand, VMReplicationRelationshipType.Simple);
        VMReplicationMode replicationMode = vMReplication.ReplicationMode;
        if (replicationMode != VMReplicationMode.Primary && replicationMode != VMReplicationMode.Replica && replicationMode != VMReplicationMode.ExtendedReplica)
        {
            VMReplication.ReportInvalidModeError("Stop-VMFailover", vMReplication);
        }
        VirtualMachine testVirtualMachine = vMReplication.TestVirtualMachine;
        VMReplicationState replicationState = vMReplication.ReplicationState;
        if (replicationState != VMReplicationState.FailedOverWaitingCompletion && replicationState != VMReplicationState.PreparedForFailover && replicationState != VMReplicationState.RecoveryInProgress && testVirtualMachine == null)
        {
            throw ExceptionHelper.CreateInvalidStateException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.OperationFailed_InvalidReplicationState, vMReplication.VMName), null, null);
        }
        if (testVirtualMachine != null)
        {
            if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_StopVMFailover_Test, operand.Name)))
            {
                if (testVirtualMachine.State == VMState.Running || testVirtualMachine.State == VMState.Paused)
                {
                    testVirtualMachine.ChangeState(VirtualMachineAction.Stop, operationWatcher);
                }
                ((IRemovable)testVirtualMachine).Remove(operationWatcher);
                flag = true;
            }
        }
        else if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_StopVMFailover, operand.Name)))
        {
            if (operand.State == VMState.Running || operand.State == VMState.Paused)
            {
                operand.ChangeState(VirtualMachineAction.Stop, operationWatcher);
            }
            VMReplicationServer.GetReplicationServer(operand.Server).RevertFailover(operand, operationWatcher);
            flag = true;
        }
        if (flag && (bool)Passthru)
        {
            operationWatcher.WriteObject(operand);
        }
    }
}

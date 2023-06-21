using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Suspend", "VMReplication", SupportsShouldProcess = true, DefaultParameterSetName = "VMName")]
[OutputType(new Type[] { typeof(VMReplication) })]
internal sealed class SuspendVMReplication : VirtualizationCmdlet<VMReplication>, ISupportsPassthrough, IVmByVMNameCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmByObjectCmdlet
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

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
    [ValidateNotNull]
    [Parameter(ParameterSetName = "VMReplication", ValueFromPipeline = true, Position = 0, Mandatory = true)]
    public VMReplication[] VMReplication { get; set; }

    [Alias(new string[] { "Relationship" })]
    [Parameter(ParameterSetName = "VMName")]
    public VMReplicationRelationshipType? ReplicationRelationshipType { get; set; }

    [Parameter]
    public SwitchParameter Passthru { get; set; }

    internal override IList<VMReplication> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        if (CurrentParameterSetIs("VMReplication"))
        {
            return VMReplication;
        }
        VMReplicationRelationshipType relationshipType = ReplicationRelationshipType.GetValueOrDefault(VMReplicationRelationshipType.Simple);
        return (from vm in ParameterResolvers.ResolveVirtualMachines(this, operationWatcher)
            select global::Microsoft.HyperV.PowerShell.VMReplication.GetVMReplication(vm, relationshipType) into replication
            where replication != null
            select replication).ToList();
    }

    internal override void ProcessOneOperand(VMReplication operand, IOperationWatcher operationWatcher)
    {
        if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_SuspendVMReplication, operand.VMName)))
        {
            ReplicationWmiState replicationWmiState = operand.ReplicationWmiState;
            if (replicationWmiState != ReplicationWmiState.WaitingToCompleteInitialReplication && replicationWmiState != ReplicationWmiState.Resynchronizing && replicationWmiState != ReplicationWmiState.Replicating)
            {
                throw ExceptionHelper.CreateInvalidStateException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.OperationFailed_InvalidReplicationState, operand.VMName), null, null);
            }
            operand.VirtualMachine.SetReplicationStateEx(operand, ReplicationWmiState.Suspended, operationWatcher);
            if ((bool)Passthru)
            {
                operationWatcher.WriteObject(operand);
            }
        }
    }
}

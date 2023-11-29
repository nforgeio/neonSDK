using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Stop", "VMReplication", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium, DefaultParameterSetName = "VMName")]
[OutputType(new Type[] { typeof(VMReplication) })]
internal sealed class StopVMReplication : VirtualizationCmdlet<VMReplication>, IVmByVMNameCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmByObjectCmdlet, ISupportsPassthrough
{
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "VMName")]
    [ValidateNotNullOrEmpty]
    public override string[] ComputerName { get; set; }

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
    [Parameter(ParameterSetName = "VMObject")]
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

    internal override void ProcessOneOperand(VMReplication replication, IOperationWatcher operationWatcher)
    {
        if (!operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_StopVMReplication, replication.VMName)))
        {
            return;
        }
        VMReplicationMode replicationMode = replication.ReplicationMode;
        if (replicationMode != VMReplicationMode.Primary && replicationMode != VMReplicationMode.Replica && replicationMode != VMReplicationMode.ExtendedReplica)
        {
            global::Microsoft.HyperV.PowerShell.VMReplication.ReportInvalidModeError("Stop-VMReplication", replication);
        }
        ReplicationWmiState replicationWmiState = replication.ReplicationWmiState;
        if ((replicationWmiState == ReplicationWmiState.WaitingForUpdateCompletion || replicationWmiState == ReplicationWmiState.UpdateCritical) && (replication.ReplicationRelationshipType == VMReplicationRelationshipType.Extended || replicationMode == VMReplicationMode.ExtendedReplica))
        {
            throw ExceptionHelper.CreateOperationFailedException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.OperationFailed_NotSupported, replication.VMName));
        }
        IVMTask iVMTask = ((replicationWmiState == ReplicationWmiState.WaitingForUpdateCompletion) ? replication.VirtualMachine.GetReplicationUpdateTask() : replication.VirtualMachine.GetReplicationResyncTask());
        VMReplicationState replicationState = replication.ReplicationState;
        if (iVMTask == null && replicationState != VMReplicationState.Resynchronizing && replicationState != VMReplicationState.WaitingForUpdateCompletion && replicationState != VMReplicationState.UpdateError)
        {
            throw ExceptionHelper.CreateInvalidStateException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.OperationFailed_InvalidReplicationState, replication.VMName), null, null);
        }
        if (iVMTask != null && iVMTask.Status == VMTaskStatus.Running && !iVMTask.IsCompleted)
        {
            iVMTask.Cancel();
            iVMTask.WaitForCompletion();
        }
        else
        {
            switch (replicationState)
            {
            case VMReplicationState.Resynchronizing:
            {
                ReplicationWmiState replicationWmiState3 = ReplicationWmiState.Disabled;
                switch (replication.ReplicationMode)
                {
                case VMReplicationMode.Primary:
                    replicationWmiState3 = ReplicationWmiState.WaitingForStartResynchronize;
                    break;
                case VMReplicationMode.Replica:
                    replicationWmiState3 = ((replication.ReplicationRelationshipType == VMReplicationRelationshipType.Extended) ? ReplicationWmiState.WaitingForStartResynchronize : ReplicationWmiState.Replicating);
                    break;
                case VMReplicationMode.ExtendedReplica:
                    replicationWmiState3 = ReplicationWmiState.Replicating;
                    break;
                }
                replication.VirtualMachine.SetReplicationStateEx(replication, replicationWmiState3, operationWatcher);
                break;
            }
            case VMReplicationState.WaitingForUpdateCompletion:
            case VMReplicationState.UpdateError:
            {
                ReplicationWmiState replicationWmiState2 = ReplicationWmiState.Disabled;
                switch (replication.ReplicationMode)
                {
                case VMReplicationMode.Primary:
                    replicationWmiState2 = ((!replication.LastReplicationTime.HasValue) ? ReplicationWmiState.Ready : ReplicationWmiState.Replicating);
                    break;
                case VMReplicationMode.Replica:
                    if (replication.ReplicationRelationshipType == VMReplicationRelationshipType.Simple)
                    {
                        replicationWmiState2 = (replication.LastReplicationTime.HasValue ? ReplicationWmiState.Replicating : ReplicationWmiState.WaitingToCompleteInitialReplication);
                    }
                    break;
                }
                replication.VirtualMachine.SetReplicationStateEx(replication, replicationWmiState2, operationWatcher);
                break;
            }
            }
        }
        if ((bool)Passthru)
        {
            operationWatcher.WriteObject(replication);
        }
    }
}

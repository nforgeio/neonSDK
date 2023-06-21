using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Start", "VMInitialReplication", DefaultParameterSetName = "VMName", ConfirmImpact = ConfirmImpact.Medium, SupportsShouldProcess = true)]
[OutputType(new Type[] { typeof(VMReplication) })]
internal sealed class StartVMInitialReplication : VirtualizationCmdlet<VMReplication>, IVmByVMNameCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmByObjectCmdlet, ISupportsPassthrough, ISupportsAsJob
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

    [Alias(new string[] { "IRLoc" })]
    [ValidateNotNullOrEmpty]
    [Parameter]
    public string DestinationPath { get; set; }

    [Alias(new string[] { "IRTime" })]
    [ValidateNotNullOrEmpty]
    [Parameter]
    public DateTime? InitialReplicationStartTime { get; set; }

    [Parameter]
    public SwitchParameter UseBackup { get; set; }

    [Parameter]
    public SwitchParameter AsJob { get; set; }

    [Parameter]
    public SwitchParameter Passthru { get; set; }

    protected override void NormalizeParameters()
    {
        base.NormalizeParameters();
        if (!string.IsNullOrEmpty(DestinationPath))
        {
            DestinationPath = PathUtility.GetFullPath(DestinationPath, base.CurrentFileSystemLocation);
        }
    }

    protected override void ValidateParameters()
    {
        base.ValidateParameters();
        if (InitialReplicationStartTime.HasValue)
        {
            if ((bool)AsJob)
            {
                throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.OperationFailed_InitialReplicationStartAndAsJobTogether);
            }
            DateTime value = InitialReplicationStartTime.Value;
            if (value < DateTime.Now)
            {
                throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.VMReplication_StartTimeOccursInPast);
            }
            if (value > DateTime.Now.AddDays(7.0))
            {
                throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.VMReplication_StartTimeOccursTooMuchInFuture);
            }
        }
        if ((InitialReplicationStartTime.HasValue || (bool)UseBackup) && !string.IsNullOrEmpty(DestinationPath))
        {
            throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.InvalidParameter_ConflictingParameters);
        }
    }

    internal override IList<VMReplication> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        if (CurrentParameterSetIs("VMReplication"))
        {
            return VMReplication;
        }
        return (from vm in ParameterResolvers.ResolveVirtualMachines(this, operationWatcher)
            select global::Microsoft.HyperV.PowerShell.VMReplication.GetVMReplication(vm, (vm.ReplicationMode == VMReplicationMode.Replica) ? VMReplicationRelationshipType.Extended : VMReplicationRelationshipType.Simple) into replication
            where replication != null
            select replication).ToList();
    }

    internal override void ProcessOneOperand(VMReplication replication, IOperationWatcher operationWatcher)
    {
        if (!operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_StartVMInitialReplication, replication.VMName)))
        {
            return;
        }
        VMReplicationMode replicationMode = replication.ReplicationMode;
        if (replicationMode != VMReplicationMode.Primary && replicationMode != VMReplicationMode.Replica)
        {
            global::Microsoft.HyperV.PowerShell.VMReplication.ReportInvalidModeError("Start-VMInitialReplication", replication);
        }
        if (replication.ReplicationWmiState != ReplicationWmiState.Ready)
        {
            throw ExceptionHelper.CreateInvalidStateException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.OperationFailed_InvalidReplicationState, replication.VMName), null, null);
        }
        DateTime valueOrDefault = InitialReplicationStartTime.GetValueOrDefault(DateTime.MinValue);
        InitialReplicationType initialReplicationType = InitialReplicationType.OverNetwork;
        if (!string.IsNullOrEmpty(DestinationPath))
        {
            initialReplicationType = InitialReplicationType.Export;
        }
        else if ((bool)UseBackup)
        {
            initialReplicationType = InitialReplicationType.UsingBackup;
        }
        VMReplicationServer.GetReplicationServer(replication.Server).StartReplication(replication.VirtualMachine, initialReplicationType, DestinationPath, valueOrDefault, operationWatcher);
        if ((bool)AsJob || initialReplicationType == InitialReplicationType.Export)
        {
            IVMTask replicationSendingInitialTask = replication.VirtualMachine.GetReplicationSendingInitialTask();
            if (replicationSendingInitialTask != null)
            {
                WatchableTask.MonitorTask(replicationSendingInitialTask, CmdletResources.Task_SendingInitialReplication, operationWatcher, null);
            }
        }
        if ((bool)Passthru)
        {
            operationWatcher.WriteObject(replication);
        }
    }
}

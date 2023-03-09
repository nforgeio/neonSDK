using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Stop", "VMInitialReplication", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium, DefaultParameterSetName = "VMName")]
[OutputType(new Type[] { typeof(VMReplication) })]
internal sealed class StopVMInitialReplication : VirtualizationCmdlet<VMReplication>, IVmByVMNameCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmByObjectCmdlet, ISupportsPassthrough
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

	[Parameter]
	public SwitchParameter Passthru { get; set; }

	internal override IList<VMReplication> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		if (CurrentParameterSetIs("VMReplication"))
		{
			return VMReplication;
		}
		return (from vm in ParameterResolvers.ResolveVirtualMachines(this, operationWatcher)
			select Microsoft.HyperV.PowerShell.VMReplication.GetVMReplication(vm, (vm.ReplicationMode == VMReplicationMode.Replica) ? VMReplicationRelationshipType.Extended : VMReplicationRelationshipType.Simple) into replication
			where replication != null
			select replication).ToList();
	}

	internal override void ProcessOneOperand(VMReplication operand, IOperationWatcher operationWatcher)
	{
		if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_StopVMInitialReplication, operand.VMName)))
		{
			VMReplicationMode replicationMode = operand.ReplicationMode;
			if (replicationMode != VMReplicationMode.Primary && replicationMode != VMReplicationMode.Replica)
			{
				Microsoft.HyperV.PowerShell.VMReplication.ReportInvalidModeError("Stop-VMInitialReplication", operand);
			}
			VMReplicationState replicationState = operand.ReplicationState;
			VirtualMachine virtualMachine = operand.VirtualMachine;
			IVMTask replicationStartTask = virtualMachine.GetReplicationStartTask();
			if (replicationStartTask == null && replicationState != VMReplicationState.InitialReplicationInProgress && replicationState != VMReplicationState.Suspended && replicationState != VMReplicationState.Error && operand.LastReplicationTime.HasValue)
			{
				throw ExceptionHelper.CreateInvalidStateException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.OperationFailed_InvalidReplicationState, operand.VMName), null, null);
			}
			if (replicationStartTask != null && replicationStartTask.Status == VMTaskStatus.Running && !replicationStartTask.IsCompleted)
			{
				replicationStartTask.Cancel();
			}
			else
			{
				virtualMachine.SetReplicationStateEx(operand, ReplicationWmiState.Ready, operationWatcher);
			}
			if ((bool)Passthru)
			{
				operationWatcher.WriteObject(operand);
			}
		}
	}
}

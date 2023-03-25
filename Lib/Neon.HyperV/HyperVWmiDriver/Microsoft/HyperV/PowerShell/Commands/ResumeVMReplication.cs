using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.HyperV.PowerShell.ExtensionMethods;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Resume", "VMReplication", ConfirmImpact = ConfirmImpact.Medium, SupportsShouldProcess = true, DefaultParameterSetName = "VMName")]
[OutputType(new Type[] { typeof(VMReplication) })]
internal sealed class ResumeVMReplication : VirtualizationCmdlet<VMReplication>, IVmByVMNameCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmByObjectCmdlet, ISupportsPassthrough, ISupportsAsJob
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

	[Alias(new string[] { "ResyncStart" })]
	[ValidateNotNull]
	[Parameter]
	public DateTime? ResynchronizeStartTime { get; set; }

	[Alias(new string[] { "Resync" })]
	[Parameter]
	public SwitchParameter Resynchronize { get; set; }

	[Parameter]
	public SwitchParameter AsJob { get; set; }

	[Parameter]
	public SwitchParameter Continue { get; set; }

	[Parameter]
	public SwitchParameter Passthru { get; set; }

	protected override void ValidateParameters()
	{
		base.ValidateParameters();
		if (ResynchronizeStartTime.HasValue)
		{
			if (!Resynchronize)
			{
				throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.InvalidParameter_ConflictingParameters);
			}
			DateTime value = ResynchronizeStartTime.Value;
			if (value < DateTime.Now)
			{
				throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.VMReplication_StartTimeOccursInPast);
			}
			if (value > DateTime.Now.AddDays(7.0))
			{
				throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.VMReplication_StartTimeOccursTooMuchInFuture);
			}
		}
	}

	internal override IList<VMReplication> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		if (CurrentParameterSetIs("VMReplication"))
		{
			return VMReplication;
		}
		VMReplicationRelationshipType relationshipType = ReplicationRelationshipType.GetValueOrDefault(VMReplicationRelationshipType.Simple);
		return ParameterResolvers.ResolveVirtualMachines(this, operationWatcher).SelectWithLogging((VirtualMachine vm) => global::Microsoft.HyperV.PowerShell.VMReplication.GetVMReplication(vm, relationshipType), operationWatcher).ToList();
	}

	internal override void ProcessOneOperand(VMReplication operand, IOperationWatcher operationWatcher)
	{
		if ((bool)Continue)
		{
			if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_ContinueVMReplication, operand.VMName)))
			{
				ProcessContinueReplication(operand, operationWatcher);
			}
		}
		else if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_ResumeVMReplication, operand.VMName)))
		{
			ProcessResumeReplication(operand, operationWatcher);
		}
	}

	private void ProcessResumeReplication(VMReplication operand, IOperationWatcher operationWatcher)
	{
		ReplicationWmiState replicationWmiState = operand.ReplicationWmiState;
		if (replicationWmiState != ReplicationWmiState.Suspended && replicationWmiState != ReplicationWmiState.WaitingForStartResynchronize && replicationWmiState != ReplicationWmiState.ResynchronizeSuspended && replicationWmiState != ReplicationWmiState.Critical && replicationWmiState != ReplicationWmiState.UpdateCritical)
		{
			throw ExceptionHelper.CreateInvalidStateException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.OperationFailed_InvalidReplicationState, operand.VMName), null, null);
		}
		VMReplicationServer replicationServer = VMReplicationServer.GetReplicationServer(operand.Server);
		if ((bool)Resynchronize)
		{
			DateTime valueOrDefault = ResynchronizeStartTime.GetValueOrDefault(DateTime.MinValue);
			replicationServer.ResynchronizeAsync(operand.VirtualMachine, valueOrDefault, operationWatcher, AsJob.IsPresent);
		}
		else
		{
			operand.VirtualMachine.SetReplicationStateEx(operand, (operand.ReplicationWmiState == ReplicationWmiState.UpdateCritical) ? ReplicationWmiState.WaitingForUpdateCompletion : ReplicationWmiState.Replicating, operationWatcher);
		}
		if (!AsJob && (bool)Passthru)
		{
			operationWatcher.WriteObject(operand);
		}
	}

	private void ProcessContinueReplication(VMReplication operand, IOperationWatcher operationWatcher)
	{
		if (operand.ReplicationMode != VMReplicationMode.Replica)
		{
			throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.VMReplication_ActionOnlyApplicableOnReplica, "Continue"));
		}
		ReplicationWmiState replicationWmiState = operand.ReplicationWmiState;
		if (replicationWmiState != ReplicationWmiState.Recovered && replicationWmiState != ReplicationWmiState.Committed)
		{
			throw ExceptionHelper.CreateInvalidStateException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.OperationFailed_InvalidReplicationState, operand.VMName), null, null);
		}
		VMReplication vMReplication = global::Microsoft.HyperV.PowerShell.VMReplication.GetVMReplication(operand.VirtualMachine, VMReplicationRelationshipType.Extended);
		if (vMReplication == null || !vMReplication.IsEnabled)
		{
			throw ExceptionHelper.CreateInvalidStateException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.OperationFailed_InvalidReplicationState, operand.VMName), null, null);
		}
		ReplicationWmiState replicationWmiState2 = operand.ReplicationWmiState;
		if (operand.FailedOverReplicationType == VMReplicationType.Planned && (replicationWmiState2 == ReplicationWmiState.Suspended || replicationWmiState2 == ReplicationWmiState.Replicating))
		{
			if (replicationWmiState2 == ReplicationWmiState.Suspended)
			{
				operand.VirtualMachine.SetReplicationStateEx(operand, ReplicationWmiState.Replicating, operationWatcher);
			}
			vMReplication.VirtualMachine.SetReplicationStateEx(vMReplication, ReplicationWmiState.SyncedReplicationComplete, operationWatcher);
		}
		VMReplicationServer replicationServer = VMReplicationServer.GetReplicationServer(operand.Server);
		if (operand.ReplicationWmiState == ReplicationWmiState.Recovered)
		{
			replicationServer.CommitFailover(operand.VirtualMachine, operationWatcher);
		}
		replicationServer.ChangeReplicationModeToPrimary(vMReplication, operationWatcher);
		if (operand.ReplicationWmiState == ReplicationWmiState.WaitingForStartResynchronize)
		{
			replicationServer.ResynchronizeAsync(operand.VirtualMachine, DateTime.MinValue, operationWatcher, AsJob.IsPresent);
		}
		if ((bool)Passthru)
		{
			operationWatcher.WriteObject(operand);
		}
	}
}

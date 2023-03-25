using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Start", "VMFailover", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, DefaultParameterSetName = "VMName")]
[OutputType(new Type[] { typeof(VirtualMachine) })]
internal sealed class StartVMFailover : VirtualizationCmdlet<VirtualMachine>, IVmByVMNameCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmByObjectCmdlet, ISupportsPassthrough, ISupportsAsJob
{
	private static class AdditionalParameterSetNames
	{
		public const string VMNameTest = "VMName_Test";

		public const string VMObjectTest = "VMObject_Test";

		public const string VMSnapshotTest = "VMSnapshot_Test";
	}

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "VMName")]
	[Parameter(ParameterSetName = "VMName_Test")]
	[ValidateNotNullOrEmpty]
	public override CimSession[] CimSession { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "VMName")]
	[Parameter(ParameterSetName = "VMName_Test")]
	[ValidateNotNullOrEmpty]
	public override string[] ComputerName { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "VMName")]
	[Parameter(ParameterSetName = "VMName_Test")]
	[ValidateNotNullOrEmpty]
	[CredentialArray]
	public override PSCredential[] Credential { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "VMName", ValueFromPipeline = true, Position = 0, Mandatory = true)]
	[Parameter(ParameterSetName = "VMName_Test", ValueFromPipeline = true, Position = 0, Mandatory = true)]
	public string[] VMName { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "VMObject", ValueFromPipeline = true, Position = 0, Mandatory = true)]
	[Parameter(ParameterSetName = "VMObject_Test", ValueFromPipeline = true, Position = 0, Mandatory = true)]
	public VirtualMachine[] VM { get; set; }

	[ValidateNotNull]
	[Parameter(Position = 0, ValueFromPipeline = true, Mandatory = true, ParameterSetName = "VMSnapshot")]
	[Parameter(Position = 0, ValueFromPipeline = true, Mandatory = true, ParameterSetName = "VMSnapshot_Test")]
	[Alias(new string[] { "VMRecoveryCheckpoint" })]
	public VMSnapshot VMRecoverySnapshot { get; set; }

	[Parameter(ParameterSetName = "VMObject_Test", Mandatory = true)]
	[Parameter(ParameterSetName = "VMName_Test", Mandatory = true)]
	[Parameter(ParameterSetName = "VMSnapshot_Test", Mandatory = true)]
	public SwitchParameter AsTest { get; set; }

	[Parameter(ParameterSetName = "VMObject")]
	[Parameter(ParameterSetName = "VMName")]
	public SwitchParameter Prepare { get; set; }

	[Parameter]
	public SwitchParameter AsJob { get; set; }

	[Parameter]
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
	public SwitchParameter Passthru { get; set; }

	public override VirtualMachineParameterType VirtualMachineParameterType
	{
		get
		{
			if (CurrentParameterSetIs("VMName_Test"))
			{
				return VirtualMachineParameterType.VMName;
			}
			if (CurrentParameterSetIs("VMObject_Test"))
			{
				return VirtualMachineParameterType.VMObject;
			}
			return base.VirtualMachineParameterType;
		}
	}

	internal override IList<VirtualMachine> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		IList<VirtualMachine> inputs = ((!CurrentParameterSetIs("VMSnapshot") && !CurrentParameterSetIs("VMSnapshot_Test")) ? ParameterResolvers.ResolveVirtualMachines(this, operationWatcher) : new VirtualMachine[1] { VMRecoverySnapshot.GetVirtualMachine() });
		return inputs.SelectWithLogging(ValidateVirtualMachineAndReplicationState, operationWatcher).ToList();
	}

	internal override void ProcessOneOperand(VirtualMachine operand, IOperationWatcher operationWatcher)
	{
		if (!operationWatcher.ShouldProcess(string.Format(format: Prepare ? CmdletResources.ShouldProcess_StartVMFailover_Prepare : ((!AsTest) ? CmdletResources.ShouldProcess_StartVMFailover : CmdletResources.ShouldProcess_StartVMFailover_Test), provider: CultureInfo.CurrentCulture, arg0: operand.Name)))
		{
			return;
		}
		if ((bool)Prepare)
		{
			VMReplicationMode replicationMode = operand.ReplicationMode;
			if (replicationMode == VMReplicationMode.Primary || replicationMode == VMReplicationMode.Replica)
			{
				VMReplication vMReplication = VMReplication.GetVMReplication(operand, (replicationMode != VMReplicationMode.Primary) ? VMReplicationRelationshipType.Extended : VMReplicationRelationshipType.Simple);
				operand.SetReplicationStateEx(vMReplication, ReplicationWmiState.SyncedReplicationComplete, operationWatcher);
			}
			else
			{
				VMReplication.ReportInvalidModeError("Start-VMFailover", VMReplication.GetVMReplication(operand, VMReplicationRelationshipType.Simple));
			}
			if ((bool)Passthru)
			{
				operationWatcher.WriteObject(operand);
			}
			return;
		}
		VMReplicationServer replicationServer = VMReplicationServer.GetReplicationServer(operand.Server);
		VirtualMachine virtualMachine;
		if ((bool)AsTest)
		{
			virtualMachine = replicationServer.TestReplicaSystem(operand, operationWatcher, VMRecoverySnapshot);
			if (virtualMachine == null)
			{
				throw ExceptionHelper.CreateOperationFailedException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.VMFailover_TestVMNotFound, operand.Name));
			}
		}
		else
		{
			VMReplication vMReplication2 = VMReplication.GetVMReplication(operand, VMReplicationRelationshipType.Simple);
			ReplicationWmiState replicationWmiState = vMReplication2.ReplicationWmiState;
			if (replicationWmiState == ReplicationWmiState.Resynchronizing || replicationWmiState == ReplicationWmiState.Suspended)
			{
				operand.SetReplicationStateEx(vMReplication2, ReplicationWmiState.Replicating, operationWatcher);
				if (vMReplication2.ReplicationWmiState == ReplicationWmiState.Resynchronizing)
				{
					operand.SetReplicationStateEx(vMReplication2, ReplicationWmiState.Replicating, operationWatcher);
				}
			}
			virtualMachine = operand;
			replicationServer.InitiateFailover(operand, operationWatcher, VMRecoverySnapshot);
		}
		if ((bool)AsTest || (bool)Passthru)
		{
			operationWatcher.WriteObject(virtualMachine);
		}
	}

	private static void ValidateStatesForPrepareFailover(VMReplication replication)
	{
		ReplicationWmiState replicationWmiState = replication.ReplicationWmiState;
		if (replicationWmiState != ReplicationWmiState.Replicating && ((replicationWmiState != ReplicationWmiState.WaitingForUpdateCompletion && replicationWmiState != ReplicationWmiState.UpdateCritical) || replication.ReplicationRelationshipType != VMReplicationRelationshipType.Extended))
		{
			throw ExceptionHelper.CreateInvalidStateException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.OperationFailed_InvalidReplicationState, replication.VMName), null, null);
		}
		if (replication.VirtualMachine.State != VMState.Off)
		{
			throw ExceptionHelper.CreateInvalidStateException(CmdletErrorMessages.OperationFailed_InvalidState, null, null);
		}
	}

	private VirtualMachine ValidateVirtualMachineAndReplicationState(VirtualMachine virtualMachine)
	{
		VMReplicationMode replicationMode = virtualMachine.ReplicationMode;
		if (replicationMode == VMReplicationMode.Replica)
		{
			ValidateVirtualMachineForReplicaFailover(virtualMachine);
		}
		else
		{
			VMReplication vMReplication = VMReplication.GetVMReplication(virtualMachine, VMReplicationRelationshipType.Simple);
			switch (replicationMode)
			{
			case VMReplicationMode.Primary:
				ValidateReplicationForPrimaryFailover(vMReplication);
				break;
			case VMReplicationMode.ExtendedReplica:
				ValidateReplicationForTertiaryFailover(vMReplication);
				break;
			default:
				VMReplication.ReportInvalidModeError("Start-VMFailover", vMReplication);
				break;
			}
		}
		return virtualMachine;
	}

	private void ValidateReplicationForPrimaryFailover(VMReplication replication)
	{
		if ((bool)AsTest || base.ParameterSetName == "VMSnapshot")
		{
			throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.VMFailover_OnlyValidParameterOnPrimary, "Prepare"));
		}
		if ((bool)Prepare)
		{
			ValidateStatesForPrepareFailover(replication);
			return;
		}
		throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.InvalidArgument_ParameterNotProvided, "Prepare"));
	}

	private void ValidateVirtualMachineForReplicaFailover(VirtualMachine virtualMachine)
	{
		VMReplication vMReplication = VMReplication.GetVMReplication(virtualMachine, Prepare ? VMReplicationRelationshipType.Extended : VMReplicationRelationshipType.Simple);
		if ((bool)Prepare)
		{
			ValidateStatesForPrepareFailover(vMReplication);
		}
		else
		{
			ValidateReplicationForFailover(vMReplication);
		}
	}

	private void ValidateReplicationForTertiaryFailover(VMReplication replication)
	{
		if ((bool)Prepare)
		{
			VMReplication.ReportInvalidModeError("Start-VMFailover", replication);
		}
		else
		{
			ValidateReplicationForFailover(replication);
		}
	}

	private void ValidateReplicationForFailover(VMReplication replication)
	{
		ReplicationWmiState replicationWmiState = replication.ReplicationWmiState;
		if (replication.VirtualMachine.State != VMState.Off)
		{
			throw ExceptionHelper.CreateInvalidStateException(CmdletErrorMessages.OperationFailed_InvalidState, null, null);
		}
		if ((replicationWmiState != ReplicationWmiState.Replicating && replicationWmiState != ReplicationWmiState.Resynchronizing && replicationWmiState != ReplicationWmiState.Suspended && replicationWmiState != ReplicationWmiState.WaitingForUpdateCompletion) || !replication.LastReplicationTime.HasValue)
		{
			throw ExceptionHelper.CreateInvalidStateException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.OperationFailed_InvalidReplicationState, replication.VMName), null, null);
		}
		if (AsTest.IsPresent && (replicationWmiState == ReplicationWmiState.Resynchronizing || replicationWmiState == ReplicationWmiState.WaitingForUpdateCompletion))
		{
			throw ExceptionHelper.CreateInvalidStateException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.OperationFailed_InvalidReplicationState, replication.VMName), null, null);
		}
		if (VMRecoverySnapshot != null)
		{
			SnapshotType snapshotType = VMRecoverySnapshot.SnapshotType;
			if (VMRecoverySnapshot.VMId != replication.VMId || snapshotType < SnapshotType.Replica || snapshotType > SnapshotType.SyncedReplica)
			{
				throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.VMFailover_InvalidSnapshot, VMRecoverySnapshot.Id, replication.VMName));
			}
		}
	}
}

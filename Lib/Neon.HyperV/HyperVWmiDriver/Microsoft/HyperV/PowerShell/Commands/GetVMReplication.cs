using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Get", "VMReplication", DefaultParameterSetName = "VMName")]
[OutputType(new Type[] { typeof(VMReplication) })]
internal sealed class GetVMReplication : VirtualizationCmdlet<VirtualMachine>, IVmByVMNameCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmByObjectCmdlet
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
	[Alias(new string[] { "Name" })]
	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "VMName", ValueFromPipeline = true, Position = 0)]
	public string[] VMName { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "VMObject", ValueFromPipeline = true, Position = 0, Mandatory = true)]
	public VirtualMachine[] VM { get; set; }

	[Alias(new string[] { "ReplicaServer" })]
	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "VMName")]
	public string ReplicaServerName { get; set; }

	[Alias(new string[] { "PrimaryServer" })]
	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "VMName")]
	public string PrimaryServerName { get; set; }

	[Alias(new string[] { "State" })]
	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "VMName")]
	public VMReplicationState? ReplicationState { get; set; }

	[Alias(new string[] { "Health" })]
	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "VMName")]
	public VMReplicationHealthState? ReplicationHealth { get; set; }

	[Alias(new string[] { "Mode" })]
	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "VMName")]
	public VMReplicationMode? ReplicationMode { get; set; }

	[Alias(new string[] { "Relationship" })]
	[Parameter]
	public VMReplicationRelationshipType? ReplicationRelationshipType { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "VMName")]
	public string TrustGroup { get; set; }

	protected override void ValidateParameters()
	{
		if (CurrentParameterSetIs("VMName") && ((ReplicationState.HasValue && ReplicationState.Value == VMReplicationState.Disabled) || (ReplicationMode.HasValue && ReplicationMode.Value == VMReplicationMode.None)))
		{
			throw ExceptionHelper.CreateInvalidStateException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.VMReplication_NoRecord_If_State_Disabled, "Get-VMReplication"), null, null);
		}
	}

	internal override IList<VirtualMachine> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		return ParameterResolvers.ResolveVirtualMachines(this, operationWatcher);
	}

	internal override void ProcessOneOperand(VirtualMachine virtualMachine, IOperationWatcher operationWatcher)
	{
		if (ReplicationRelationshipType.HasValue)
		{
			ProcessVirtualMachineForReplicationType(virtualMachine, ReplicationRelationshipType.Value, isFirstCheck: true, operationWatcher);
			return;
		}
		ProcessVirtualMachineForReplicationType(virtualMachine, VMReplicationRelationshipType.Simple, isFirstCheck: true, operationWatcher);
		ProcessVirtualMachineForReplicationType(virtualMachine, VMReplicationRelationshipType.Extended, isFirstCheck: false, operationWatcher);
	}

	private void ProcessVirtualMachineForReplicationType(VirtualMachine virtualMachine, VMReplicationRelationshipType replicationRelationshipType, bool isFirstCheck, IOperationWatcher operationWatcher)
	{
		VMReplication vMReplication = VMReplication.GetVMReplication(virtualMachine, replicationRelationshipType);
		if (vMReplication == null || !vMReplication.IsEnabled)
		{
			if (isFirstCheck && (VMName != null || VM != null))
			{
				throw ExceptionHelper.CreateOperationFailedException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.VMReplication_NotEnabled, virtualMachine.Name));
			}
		}
		else if (ParameterResolvers.ReplicationCmdlets.MatchReplicationSet(vMReplication, ReplicaServerName, PrimaryServerName, ReplicationState, ReplicationHealth, ReplicationMode, TrustGroup))
		{
			operationWatcher.WriteObject(vMReplication);
		}
	}
}

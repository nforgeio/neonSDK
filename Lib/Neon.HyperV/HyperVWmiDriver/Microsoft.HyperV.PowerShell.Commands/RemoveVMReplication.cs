using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
[Cmdlet("Remove", "VMReplication", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium, DefaultParameterSetName = "VMName")]
[OutputType(new Type[] { typeof(VMReplication) })]
internal sealed class RemoveVMReplication : VirtualizationCmdlet<VirtualMachine>, ISupportsPassthrough, IVmByVMNameCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet
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

	internal override IList<VirtualMachine> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		if (CurrentParameterSetIs("VMReplication"))
		{
			return VMReplication.Select((VMReplication replication) => replication.VirtualMachine).ToList();
		}
		return ParameterResolvers.ResolveVirtualMachines(this, operationWatcher);
	}

	internal override void ProcessOneOperand(VirtualMachine operand, IOperationWatcher operationWatcher)
	{
		if (!operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_RemoveVMReplication, operand.Name)))
		{
			return;
		}
		VMReplication vMReplication;
		if (ReplicationRelationshipType.HasValue && ReplicationRelationshipType.Value == VMReplicationRelationshipType.Extended)
		{
			vMReplication = Microsoft.HyperV.PowerShell.VMReplication.GetVMReplication(operand, VMReplicationRelationshipType.Extended);
			if (operand.ReplicationMode != VMReplicationMode.Replica && (operand.ReplicationMode != 0 || (vMReplication != null && vMReplication.ReplicationWmiState != ReplicationWmiState.Critical)))
			{
				throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.VMReplication_ActionOnlyApplicableOnReplica, "ReplicationRelationshipType"));
			}
		}
		else
		{
			vMReplication = Microsoft.HyperV.PowerShell.VMReplication.GetVMReplication(operand, VMReplicationRelationshipType.Simple);
		}
		if (vMReplication == null || vMReplication.ReplicationState == VMReplicationState.Disabled)
		{
			throw ExceptionHelper.CreateInvalidStateException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.OperationFailed_InvalidReplicationState, operand.Name), null, null);
		}
		if (vMReplication.IsReplicatingToExternalProvider)
		{
			throw ExceptionHelper.CreateOperationFailedException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.OperationFailed_NotSupportedForExternalReplicationProvider, operand.Name));
		}
		VMReplicationServer.GetReplicationServer(operand.Server).RemoveReplicationRelationshipEx(vMReplication, operationWatcher);
		if ((bool)Passthru)
		{
			operationWatcher.WriteObject(vMReplication);
		}
	}
}

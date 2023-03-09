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

[Cmdlet("Import", "VMInitialReplication", ConfirmImpact = ConfirmImpact.Medium, SupportsShouldProcess = true, DefaultParameterSetName = "VMName")]
[OutputType(new Type[] { typeof(VMReplication) })]
internal sealed class ImportVMReplication : VirtualizationCmdlet<VMReplication>, ISupportsAsJob, ISupportsPassthrough, IVmByVMNameCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmByObjectCmdlet
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

	[Alias(new string[] { "IRLoc" })]
	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true, Position = 1)]
	public string Path { get; set; }

	[Parameter]
	public SwitchParameter AsJob { get; set; }

	[Parameter]
	public SwitchParameter Passthru { get; set; }

	protected override void NormalizeParameters()
	{
		base.NormalizeParameters();
		if (!string.IsNullOrEmpty(Path))
		{
			Path = PathUtility.GetFullPath(Path, base.CurrentFileSystemLocation);
		}
	}

	internal override IList<VMReplication> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		IEnumerable<VMReplication> inputs = ((!CurrentParameterSetIs("VMReplication")) ? (from vm in ParameterResolvers.ResolveVirtualMachines(this, operationWatcher)
			select Microsoft.HyperV.PowerShell.VMReplication.GetVMReplication(vm, VMReplicationRelationshipType.Simple) into replication
			where replication != null
			select replication) : VMReplication);
		return inputs.SelectWithLogging(SelectValidReplicationObject, operationWatcher).ToList();
	}

	internal override void ProcessOneOperand(VMReplication operand, IOperationWatcher operationWatcher)
	{
		if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_ImportVMInitialReplication, operand.VMName)))
		{
			VMReplicationServer.GetReplicationServer(operand.Server).ImportInitialReplica(operand.VirtualMachine, Path, operationWatcher);
			if ((bool)Passthru)
			{
				operationWatcher.WriteObject(operand);
			}
		}
	}

	private static VMReplication SelectValidReplicationObject(VMReplication replication)
	{
		VMReplicationMode replicationMode = replication.ReplicationMode;
		if (replicationMode != VMReplicationMode.Replica && replicationMode != VMReplicationMode.ExtendedReplica)
		{
			Microsoft.HyperV.PowerShell.VMReplication.ReportInvalidModeError("Import-VMInitialReplication", replication);
		}
		if (replication.ReplicationWmiState != ReplicationWmiState.WaitingToCompleteInitialReplication)
		{
			throw ExceptionHelper.CreateInvalidStateException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.OperationFailed_InvalidReplicationState, replication.VMName), null, null);
		}
		return replication;
	}
}

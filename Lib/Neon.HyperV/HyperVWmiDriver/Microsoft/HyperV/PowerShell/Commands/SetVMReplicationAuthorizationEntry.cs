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

[Cmdlet("Set", "VMReplicationAuthorizationEntry", ConfirmImpact = ConfirmImpact.Medium, SupportsShouldProcess = true, DefaultParameterSetName = "Name")]
[OutputType(new Type[] { typeof(VMReplicationAuthorizationEntry) })]
internal sealed class SetVMReplicationAuthorizationEntry : VirtualizationCmdlet<VMReplicationAuthorizationEntry>, ISupportsPassthrough
{
	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "Name")]
	[ValidateNotNullOrEmpty]
	public override CimSession[] CimSession { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "Name")]
	[ValidateNotNullOrEmpty]
	public override string[] ComputerName { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "Name")]
	[ValidateNotNullOrEmpty]
	[CredentialArray]
	public override PSCredential[] Credential { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
	[Alias(new string[] { "VMRepAuthEntry" })]
	[ValidateNotNull]
	[Parameter(Mandatory = true, ValueFromPipeline = true, Position = 0, ParameterSetName = "Object")]
	public VMReplicationAuthorizationEntry[] VMReplicationAuthorizationEntry { get; set; }

	[Alias(new string[] { "AllowedPS" })]
	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true, ValueFromPipeline = true, Position = 0, ParameterSetName = "Name")]
	public string AllowedPrimaryServer { get; set; }

	[Alias(new string[] { "StorageLoc" })]
	[Parameter(Position = 1)]
	[ValidateNotNullOrEmpty]
	public string ReplicaStorageLocation { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(Position = 2)]
	public string TrustGroup { get; set; }

	[Parameter]
	public SwitchParameter Passthru { get; set; }

	protected override void NormalizeParameters()
	{
		base.NormalizeParameters();
		if (!string.IsNullOrEmpty(ReplicaStorageLocation))
		{
			ReplicaStorageLocation = PathUtility.GetFullPath(ReplicaStorageLocation, base.CurrentFileSystemLocation);
		}
	}

	internal override IList<VMReplicationAuthorizationEntry> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		if (CurrentParameterSetIs("Object"))
		{
			return VMReplicationAuthorizationEntry;
		}
		return (from entry in ParameterResolvers.GetServers(this, operationWatcher).SelectWithLogging(VMReplicationServer.GetReplicationServer, operationWatcher).SelectMany((VMReplicationServer server) => server.AuthorizationEntries)
			where string.Equals(entry.AllowedPrimaryServer, AllowedPrimaryServer, StringComparison.CurrentCultureIgnoreCase)
			select entry).ToList();
	}

	internal override void ProcessOneOperand(VMReplicationAuthorizationEntry operand, IOperationWatcher operationWatcher)
	{
		if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_SetVMReplicationAuthorizationEntry, operand.AllowedPrimaryServer)))
		{
			if (ReplicaStorageLocation != null)
			{
				operand.ReplicaStorageLocation = ReplicaStorageLocation;
			}
			if (TrustGroup != null)
			{
				operand.TrustGroup = TrustGroup;
			}
			((IUpdatable)operand).Put(operationWatcher);
			if ((bool)Passthru)
			{
				operationWatcher.WriteObject(operand);
			}
		}
	}
}

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

[Cmdlet("Remove", "VMReplicationAuthorizationEntry", ConfirmImpact = ConfirmImpact.Medium, SupportsShouldProcess = true, DefaultParameterSetName = "PrimaryServerName")]
[OutputType(new Type[] { typeof(VMReplicationAuthorizationEntry) })]
internal sealed class RemoveVMReplicationAuthorizationEntry : VirtualizationCmdlet<VMReplicationAuthorizationEntry>, ISupportsPassthrough
{
	private static class ParameterSetNames
	{
		public const string PrimaryServerName = "PrimaryServerName";

		public const string TrustGroup = "TrustGroup";

		public const string Object = "Object";
	}

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "PrimaryServerName")]
	[Parameter(ParameterSetName = "TrustGroup")]
	[ValidateNotNullOrEmpty]
	public override CimSession[] CimSession { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "PrimaryServerName")]
	[Parameter(ParameterSetName = "TrustGroup")]
	[ValidateNotNullOrEmpty]
	public override string[] ComputerName { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "PrimaryServerName")]
	[Parameter(ParameterSetName = "TrustGroup")]
	[ValidateNotNullOrEmpty]
	[CredentialArray]
	public override PSCredential[] Credential { get; set; }

	[Alias(new string[] { "AllowedPS" })]
	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true, ValueFromPipeline = true, Position = 0, ParameterSetName = "PrimaryServerName")]
	public string AllowedPrimaryServer { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
	[Alias(new string[] { "VMRepAuthEntry" })]
	[ValidateNotNull]
	[Parameter(Mandatory = true, ValueFromPipeline = true, Position = 0, ParameterSetName = "Object")]
	public VMReplicationAuthorizationEntry[] VMReplicationAuthorizationEntry { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true, ValueFromPipeline = true, Position = 0, ParameterSetName = "TrustGroup")]
	public string TrustGroup { get; set; }

	[Parameter]
	public SwitchParameter Passthru { get; set; }

	internal override IList<VMReplicationAuthorizationEntry> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		IList<VMReplicationAuthorizationEntry> list;
		if (CurrentParameterSetIs("Object"))
		{
			list = VMReplicationAuthorizationEntry;
		}
		else
		{
			IEnumerable<VMReplicationAuthorizationEntry> source = ParameterResolvers.GetServers(this, operationWatcher).SelectWithLogging(VMReplicationServer.GetReplicationServer, operationWatcher).SelectMany((VMReplicationServer server) => server.AuthorizationEntries);
			list = ((!CurrentParameterSetIs("PrimaryServerName")) ? source.Where((VMReplicationAuthorizationEntry entry) => WildcardPatternMatcher.IsPatternMatching(TrustGroup, entry.TrustGroup)).ToList() : source.Where((VMReplicationAuthorizationEntry entry) => string.Equals(entry.AllowedPrimaryServer, AllowedPrimaryServer, StringComparison.OrdinalIgnoreCase)).ToList());
		}
		if (list.Count == 0)
		{
			if (!string.IsNullOrEmpty(AllowedPrimaryServer))
			{
				throw ExceptionHelper.CreateObjectNotFoundException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.VMReplicationAuthorizationEntry_NotFoundByServerName, AllowedPrimaryServer), null);
			}
			if (!string.IsNullOrEmpty(TrustGroup))
			{
				throw ExceptionHelper.CreateObjectNotFoundException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.VMReplicationAuthorizationEntry_NotFoundByTrustGroup, TrustGroup), null);
			}
		}
		return list;
	}

	internal override void ProcessOneOperand(VMReplicationAuthorizationEntry operand, IOperationWatcher operationWatcher)
	{
		if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_RemoveVMReplicationAuthorizationEntry, operand.Server, operand.AllowedPrimaryServer)))
		{
			((IRemovable)operand).Remove(operationWatcher);
			if ((bool)Passthru)
			{
				operationWatcher.WriteObject(operand);
			}
		}
	}
}

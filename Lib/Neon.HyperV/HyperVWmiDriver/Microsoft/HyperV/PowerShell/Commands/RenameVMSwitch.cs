using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.Management.Infrastructure;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Rename", "VMSwitch", DefaultParameterSetName = "SwitchName", SupportsShouldProcess = true)]
[OutputType(new Type[] { typeof(VMSwitch) })]
internal sealed class RenameVMSwitch : VirtualizationCmdlet<VMSwitch>, ISupportsPassthrough
{
	internal static class ParameterSetNames
	{
		public const string SwitchObject = "SwitchObject";

		public const string SwitchName = "SwitchName";
	}

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "SwitchName")]
	[ValidateNotNullOrEmpty]
	public override CimSession[] CimSession { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "SwitchName")]
	[ValidateNotNullOrEmpty]
	public override string[] ComputerName { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "SwitchName")]
	[ValidateNotNullOrEmpty]
	[CredentialArray]
	public override PSCredential[] Credential { get; set; }

	[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "SwitchObject")]
	[ValidateNotNull]
	[ValidateNotNullOrEmpty]
	public VMSwitch VMSwitch { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "SwitchName")]
	[Alias(new string[] { "SwitchName" })]
	public string Name { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true, Position = 2)]
	public string NewName { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
	[Parameter]
	public SwitchParameter Passthru { get; set; }

	internal override IList<VMSwitch> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		if (IsParameterSpecified("VMSwitch"))
		{
			return new VMSwitch[1] { VMSwitch };
		}
		IEnumerable<Server> servers = ParameterResolvers.GetServers(this, operationWatcher);
		string[] requestedSwitchNames = new string[1] { Name };
		return VMSwitch.GetSwitchesByNamesAndServers(servers, requestedSwitchNames, allowWildcards: true, ErrorDisplayMode.WriteError, operationWatcher);
	}

	internal override void ProcessOneOperand(VMSwitch operand, IOperationWatcher operationWatcher)
	{
		if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_RenameVMSwitch, operand.Name, NewName)))
		{
			operand.Name = NewName;
			((IUpdatable)operand).Put(operationWatcher);
			if (Passthru.IsPresent)
			{
				operationWatcher.WriteObject(operand);
			}
		}
	}
}

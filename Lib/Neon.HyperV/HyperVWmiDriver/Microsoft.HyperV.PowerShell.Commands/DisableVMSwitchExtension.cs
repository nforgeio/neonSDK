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

[Cmdlet("Disable", "VMSwitchExtension", DefaultParameterSetName = "ExtensionName")]
[OutputType(new Type[] { typeof(VMSwitchExtension) })]
internal sealed class DisableVMSwitchExtension : VirtualizationCmdlet<VMSwitchExtension>
{
	private static class ParameterSetNames
	{
		public const string ExtensionName = "ExtensionName";

		public const string ExtensionNameSwitchName = "ExtensionNameSwitchName";

		public const string ExtensionNameSwitchObject = "ExtensionNameSwitchObject";

		public const string ExtensionObject = "ExtensionObject";
	}

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "ExtensionName", ValueFromPipelineByPropertyName = true)]
	[Parameter(ParameterSetName = "ExtensionNameSwitchName", ValueFromPipelineByPropertyName = true)]
	[ValidateNotNullOrEmpty]
	public override CimSession[] CimSession { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "ExtensionName")]
	[Parameter(ParameterSetName = "ExtensionNameSwitchName")]
	[Alias(new string[] { "PSComputerName" })]
	[ValidateNotNullOrEmpty]
	public override string[] ComputerName { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "ExtensionName")]
	[Parameter(ParameterSetName = "ExtensionNameSwitchName")]
	[ValidateNotNullOrEmpty]
	[CredentialArray]
	public override PSCredential[] Credential { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Array is more user friendly, and this is a user input parameter.")]
	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = false, ParameterSetName = "ExtensionName")]
	[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = false, ParameterSetName = "ExtensionNameSwitchName")]
	[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = false, ParameterSetName = "ExtensionNameSwitchObject")]
	public string[] Name { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Array is more user friendly, and this is a user input parameter.")]
	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "ExtensionObject")]
	public VMSwitchExtension[] VMSwitchExtension { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Array is more user friendly, and this is a user input parameter.")]
	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true, Position = 1, ValueFromPipeline = false, ParameterSetName = "ExtensionNameSwitchName")]
	public string[] VMSwitchName { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec, and array is more user friendly as this is an user input parameter.")]
	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true, Position = 1, ValueFromPipeline = true, ParameterSetName = "ExtensionNameSwitchObject")]
	public VMSwitch[] VMSwitch { get; set; }

	internal override IList<VMSwitchExtension> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		if (CurrentParameterSetIs("ExtensionObject"))
		{
			return VMSwitchExtension;
		}
		IEnumerable<VMSwitchExtension> source = EnumerateSwitches(operationWatcher).SelectManyWithLogging((VMSwitch virtualSwitch) => virtualSwitch.Extensions, operationWatcher);
		if (!Name.IsNullOrEmpty())
		{
			WildcardPatternMatcher matcher = new WildcardPatternMatcher(Name);
			source = source.Where((VMSwitchExtension extension) => matcher.MatchesAny(extension.Name));
		}
		return source.ToList();
	}

	internal override void ProcessOneOperand(VMSwitchExtension operand, IOperationWatcher operationWatcher)
	{
		if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_DisableVMSwitchExtension, operand.Name)))
		{
			operand.Enabled = false;
			((IUpdatable)operand).Put(operationWatcher);
			operationWatcher.WriteObject(operand);
		}
	}

	private IEnumerable<VMSwitch> EnumerateSwitches(IOperationWatcher operationWatcher)
	{
		if (CurrentParameterSetIs("ExtensionNameSwitchObject"))
		{
			return VMSwitch;
		}
		return Microsoft.HyperV.PowerShell.VMSwitch.GetSwitchesByNamesAndServers(ParameterResolvers.GetServers(this, operationWatcher), VMSwitchName, allowWildcards: true, ErrorDisplayMode.WriteWarning, operationWatcher);
	}
}

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Get", "VMSwitchTeam", DefaultParameterSetName = "Name")]
[OutputType(new Type[] { typeof(VMSwitchTeam) })]
internal sealed class GetVMSwitchTeam : VirtualizationCmdlet<VMSwitchTeam>
{
	[Parameter(Position = 0, ParameterSetName = "Name")]
	[Alias(new string[] { "SwitchName" })]
	[ValidateNotNullOrEmpty]
	public string Name { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec, and array is more user friendly as this is an user input parameter.")]
	[ValidateNotNullOrEmpty]
	[Parameter(Position = 0, ValueFromPipeline = true, ParameterSetName = "SwitchObject")]
	public VMSwitch[] VMSwitch { get; set; }

	internal override IList<VMSwitchTeam> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		IEnumerable<Server> servers = ParameterResolvers.GetServers(this, operationWatcher);
		string[] requestedSwitchNames = (string.IsNullOrEmpty(Name) ? null : new string[1] { Name });
		IEnumerable<VMSwitchTeam> source = ((!IsParameterSpecified("VMSwitch")) ? (from vmSwitch in Microsoft.HyperV.PowerShell.VMSwitch.GetSwitchesByNamesAndServers(servers, requestedSwitchNames, allowWildcards: true, ErrorDisplayMode.WriteWarning, operationWatcher)
			select new VMSwitchTeam(vmSwitch)) : VMSwitch.Select((VMSwitch s) => new VMSwitchTeam(s)));
		List<VMSwitchTeam> list = source.ToList();
		VMSwitchTeam vMSwitchTeam = list.FirstOrDefault((VMSwitchTeam vmSwitchTeam) => !vmSwitchTeam.m_VMSwitch.EmbeddedTeamingEnabled);
		if (vMSwitchTeam != null)
		{
			throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.InvalidParameter_VMSwitchIsNotTeamingEnabled, vMSwitchTeam.Name));
		}
		return list;
	}

	internal override void ValidateOperandList(IList<VMSwitchTeam> operands, IOperationWatcher operationWatcher)
	{
		if (CurrentParameterSetIs("Id") && !operands.Any())
		{
			throw ExceptionHelper.CreateObjectNotFoundException(CmdletErrorMessages.VMSwitch_NoSwitchFound, null);
		}
	}

	internal override void ProcessOneOperand(VMSwitchTeam operand, IOperationWatcher operationWatcher)
	{
		operationWatcher.WriteObject(operand);
	}
}

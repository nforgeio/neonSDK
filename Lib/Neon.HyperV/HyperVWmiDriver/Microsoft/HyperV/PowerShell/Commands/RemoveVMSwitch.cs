using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Management.Infrastructure;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Remove", "VMSwitch", DefaultParameterSetName = "Name", SupportsShouldProcess = true)]
internal sealed class RemoveVMSwitch : VirtualizationCmdlet<object>, ISupportsForce
{
	private class PoolToSwitchesPair : Tuple<VMEthernetResourcePool, IEnumerable<VMSwitch>>
	{
		public VMEthernetResourcePool Pool => base.Item1;

		public IEnumerable<VMSwitch> Switches => base.Item2;

		public PoolToSwitchesPair(VMEthernetResourcePool pool, IEnumerable<VMSwitch> switches)
			: base(pool, switches)
		{
		}
	}

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "Name")]
	public override CimSession[] CimSession { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "Name")]
	public override string[] ComputerName { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "Name")]
	[CredentialArray]
	public override PSCredential[] Credential { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec and arrays are easier to use.")]
	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "Name")]
	[Alias(new string[] { "SwitchName" })]
	public string[] Name { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec and arrays are easier to use.")]
	[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "SwitchObject")]
	public VMSwitch[] VMSwitch { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Arrays are more user friendly, and this is by spec.")]
	[ValidateNotNullOrEmpty]
	[Parameter(Position = 1)]
	public string[] ResourcePoolName { get; set; }

	[Parameter]
	public SwitchParameter Force { get; set; }

	internal override IList<object> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		if (IsParameterSpecified("ResourcePoolName"))
		{
			IList<PoolToSwitchesPair> list = new List<PoolToSwitchesPair>();
			VMResourcePoolType[] ethernetResourceType = new VMResourcePoolType[1] { VMResourcePoolType.Ethernet };
			foreach (VMEthernetResourcePool item in ParameterResolvers.GetServers(this, operationWatcher).SelectManyWithLogging((Server server) => VMResourcePool.GetVMResourcePools(server, ResourcePoolName, allowWildcards: true, ethernetResourceType), operationWatcher).Cast<VMEthernetResourcePool>())
			{
				IEnumerable<VMSwitch> switches = ((!CurrentParameterSetIs("Name")) ? VMSwitch : item.GetSwitchesByNames(Name, allowWildcards: true, ErrorDisplayMode.WriteError, operationWatcher));
				list.Add(new PoolToSwitchesPair(item, switches));
			}
			return list.Cast<object>().ToList();
		}
		IEnumerable<VMSwitch> source = ((!CurrentParameterSetIs("Name")) ? VMSwitch : global::Microsoft.HyperV.PowerShell.VMSwitch.GetSwitchesByNamesAndServers(ParameterResolvers.GetServers(this, operationWatcher), Name, allowWildcards: true, ErrorDisplayMode.WriteError, operationWatcher));
		return source.Cast<object>().ToList();
	}

	internal override void ProcessOneOperand(object operand, IOperationWatcher operationWatcher)
	{
		if (IsParameterSpecified("ResourcePoolName"))
		{
			PoolToSwitchesPair poolToSwitchesPair = (PoolToSwitchesPair)operand;
			VMEthernetResourcePool pool = poolToSwitchesPair.Pool;
			if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_RemoveVMSwitch_FromPool, pool.Name)) && operationWatcher.ShouldContinue(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldContinue_RemoveVMSwitch_FromPool, pool.Name)))
			{
				pool.RemoveSwitches(poolToSwitchesPair.Switches, operationWatcher);
			}
		}
		else
		{
			VMSwitch vMSwitch = operand as VMSwitch;
			if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_RemoveVMSwitch, vMSwitch.Name)) && operationWatcher.ShouldContinue(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldContinue_RemoveVMSwitch, vMSwitch.Name)))
			{
				((IRemovable)vMSwitch).Remove(operationWatcher);
			}
		}
	}
}

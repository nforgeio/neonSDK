using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Management.Infrastructure;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Add", "VMSwitch", DefaultParameterSetName = "NetworkByName")]
internal sealed class AddVMSwitch : VirtualizationCmdlet<Tuple<VMEthernetResourcePool, IEnumerable<VMSwitch>>>
{
	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "NetworkByName")]
	[ValidateNotNullOrEmpty]
	public override CimSession[] CimSession { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "NetworkByName")]
	[ValidateNotNullOrEmpty]
	public override string[] ComputerName { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "NetworkByName")]
	[ValidateNotNullOrEmpty]
	[CredentialArray]
	public override PSCredential[] Credential { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Arrays are more user friendly, and this is by spec.")]
	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "NetworkByName")]
	[Alias(new string[] { "SwitchName" })]
	public string[] Name { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Arrays are more user friendly, and this is by spec.")]
	[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "NetworkByObject")]
	public VMSwitch[] VMSwitch { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Arrays are more user friendly, and this is by spec.")]
	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true, Position = 1)]
	public string[] ResourcePoolName { get; set; }

	internal override IList<Tuple<VMEthernetResourcePool, IEnumerable<VMSwitch>>> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		VMResourcePoolType[] ethernetResourceType = new VMResourcePoolType[1] { VMResourcePoolType.Ethernet };
		List<Server> list = ParameterResolvers.GetServers(this, operationWatcher).ToList();
		IEnumerable<VMEthernetResourcePool> source = list.SelectManyWithLogging((Server server) => VMResourcePool.GetVMResourcePools(server, ResourcePoolName, allowWildcards: true, ethernetResourceType), operationWatcher).Cast<VMEthernetResourcePool>();
		IList<VMSwitch> switches;
		if (CurrentParameterSetIs("NetworkByName"))
		{
			switches = global::Microsoft.HyperV.PowerShell.VMSwitch.GetSwitchesByNamesAndServers(list, Name, allowWildcards: true, ErrorDisplayMode.WriteError, operationWatcher);
		}
		else
		{
			switches = VMSwitch;
		}
		return source.Select((VMEthernetResourcePool pool) => Tuple.Create(pool, switches.AsEnumerable())).ToList();
	}

	internal override void ProcessOneOperand(Tuple<VMEthernetResourcePool, IEnumerable<VMSwitch>> operand, IOperationWatcher operationWatcher)
	{
		VMEthernetResourcePool item = operand.Item1;
		IEnumerable<VMSwitch> item2 = operand.Item2;
		item.AddSwitches(item2, operationWatcher);
	}
}

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Management.Infrastructure;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Get", "VMNetworkAdapter", DefaultParameterSetName = "VMName")]
[OutputType(new Type[] { typeof(VMNetworkAdapterBase) })]
internal sealed class GetVMNetworkAdapter : VirtualizationCmdlet<VMNetworkAdapterBase>, IVMObjectOrVMNameCmdlet, IVmByObjectCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmByVMNameCmdlet, IVMSnapshotCmdlet, IVMInternalNetworkAdapterBySwitchNameCmdlet, IVMNetworkAdapterBaseCmdlet
{
	internal static class ParameterSetNames
	{
		public const string All = "All";
	}

	[Parameter(ParameterSetName = "VMName")]
	[Parameter(ParameterSetName = "VMObject")]
	public bool IsLegacy { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "VMName")]
	[Parameter(ParameterSetName = "ManagementOS")]
	[Parameter(ParameterSetName = "All")]
	[ValidateNotNullOrEmpty]
	public override CimSession[] CimSession { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "VMName")]
	[Parameter(ParameterSetName = "ManagementOS")]
	[Parameter(ParameterSetName = "All")]
	[ValidateNotNullOrEmpty]
	public override string[] ComputerName { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "VMName")]
	[Parameter(ParameterSetName = "ManagementOS")]
	[Parameter(ParameterSetName = "All")]
	[ValidateNotNullOrEmpty]
	[CredentialArray]
	public override PSCredential[] Credential { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "VMObject", ValueFromPipeline = true, Position = 0, Mandatory = true)]
	public VirtualMachine[] VM { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "VMName", ValueFromPipeline = true, Mandatory = true, Position = 0)]
	public string[] VMName { get; set; }

	[Parameter(Position = 1)]
	[Alias(new string[] { "VMNetworkAdapterName" })]
	public string Name { get; set; }

	[Parameter(ParameterSetName = "All", Mandatory = true)]
	public SwitchParameter All { get; set; }

	[Parameter(ParameterSetName = "ManagementOS", Mandatory = true)]
	public SwitchParameter ManagementOS { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "VMSnapshot")]
	[Alias(new string[] { "VMCheckpoint" })]
	public VMSnapshot VMSnapshot { get; set; }

	[Parameter(ParameterSetName = "ManagementOS")]
	[ValidateNotNullOrEmpty]
	public string SwitchName { get; set; }

	internal override IList<VMNetworkAdapterBase> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		IEnumerable<VMNetworkAdapterBase> enumerable;
		if (CurrentParameterSetIs("All"))
		{
			List<Server> servers = ParameterResolvers.GetServers(this, operationWatcher).ToList();
			IEnumerable<VMNetworkAdapterBase> first = VirtualizationObjectLocator.GetVMHosts(servers, operationWatcher).SelectManyWithLogging((VMHost host) => host.InternalNetworkAdapters, operationWatcher);
			IEnumerable<VMNetworkAdapter> second = VirtualizationObjectLocator.GetVirtualMachinesByNamesAndServers(servers, null, allowWildcards: false, ErrorDisplayMode.WriteError, operationWatcher).SelectManyWithLogging((VirtualMachine vm) => vm.GetNetworkAdapters(), operationWatcher);
			enumerable = first.Concat(second);
			if (!string.IsNullOrEmpty(Name))
			{
				enumerable = ParameterResolvers.FilterNetworkAdaptersByName(enumerable, Name);
			}
		}
		else
		{
			enumerable = ParameterResolvers.ResolveNetworkAdapters(this, Name, operationWatcher);
		}
		return enumerable.ToList();
	}

	internal override void ProcessOneOperand(VMNetworkAdapterBase operand, IOperationWatcher operationWatcher)
	{
		operationWatcher.WriteObject(operand);
	}
}

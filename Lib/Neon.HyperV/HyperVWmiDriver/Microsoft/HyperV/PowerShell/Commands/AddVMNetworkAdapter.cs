using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.Management.Infrastructure;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Add", "VMNetworkAdapter", SupportsShouldProcess = true, DefaultParameterSetName = "VMName")]
[OutputType(new Type[]
{
	typeof(VMNetworkAdapter),
	typeof(VMInternalNetworkAdapter)
})]
internal sealed class AddVMNetworkAdapter : VirtualizationCmdlet<IVMNetworkAdapterOwner>, IVMObjectOrVMNameCmdlet, IVmByObjectCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmByVMNameCmdlet, ISupportsPassthrough
{
	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "VMName")]
	[Parameter(ParameterSetName = "ManagementOS")]
	[ValidateNotNullOrEmpty]
	public override CimSession[] CimSession { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "VMName")]
	[Parameter(ParameterSetName = "ManagementOS")]
	[ValidateNotNullOrEmpty]
	public override string[] ComputerName { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "VMName")]
	[Parameter(ParameterSetName = "ManagementOS")]
	[ValidateNotNullOrEmpty]
	[CredentialArray]
	public override PSCredential[] Credential { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "VMName", ValueFromPipeline = true, Position = 0, Mandatory = true)]
	public string[] VMName { get; set; }

	[Parameter(ParameterSetName = "ManagementOS")]
	public SwitchParameter ManagementOS { get; set; }

	[Parameter]
	[ValidateNotNullOrEmpty]
	public string SwitchName { get; set; }

	[Parameter(ParameterSetName = "VMName")]
	[Parameter(ParameterSetName = "VMObject")]
	public bool IsLegacy { get; set; }

	[Parameter]
	[ValidateNotNullOrEmpty]
	[Alias(new string[] { "VMNetworkAdapterName" })]
	public string Name { get; set; }

	[Parameter]
	public SwitchParameter DynamicMacAddress { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "The AllowPacketDirect parameter is temporarily disabled for the RS1 release.")]
	private SwitchParameter AllowPacketDirect { get; set; }

	[ValidateNotNull]
	[Parameter]
	public bool NumaAwarePlacement { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter]
	public string StaticMacAddress { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
	[Parameter]
	public SwitchParameter Passthru { get; set; }

	[ValidateNotNull]
	[Parameter(ParameterSetName = "VMName")]
	[Parameter(ParameterSetName = "VMObject")]
	public string ResourcePoolName { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "VMObject", ValueFromPipeline = true, Position = 0, Mandatory = true)]
	public VirtualMachine[] VM { get; set; }

	[Parameter]
	public OnOffState DeviceNaming { get; set; }

	protected override void ValidateParameters()
	{
		base.ValidateParameters();
		ValidateMutuallyExclusiveParameters("StaticMacAddress", "DynamicMacAddress");
	}

	protected override void NormalizeParameters()
	{
		base.NormalizeParameters();
		if (!string.IsNullOrEmpty(StaticMacAddress))
		{
			StaticMacAddress = ParameterResolvers.ValidateAndNormalizeMacAddress(StaticMacAddress, "StaticMacAddress");
		}
		if (StaticMacAddress == null && !DynamicMacAddress.IsPresent)
		{
			DynamicMacAddress = true;
		}
	}

	internal override IList<IVMNetworkAdapterOwner> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		if (CurrentParameterSetIs("ManagementOS"))
		{
			IEnumerable<Server> servers = ParameterResolvers.GetServers(this, operationWatcher);
			string[] requestedSwitchNames = (string.IsNullOrEmpty(SwitchName) ? null : new string[1] { SwitchName });
			return VMSwitch.GetSwitchesByNamesAndServers(servers, requestedSwitchNames, allowWildcards: true, ErrorDisplayMode.WriteError, operationWatcher).OfType<IVMNetworkAdapterOwner>().ToList();
		}
		return ParameterResolvers.ResolveVirtualMachines(this, operationWatcher).OfType<IVMNetworkAdapterOwner>().ToList();
	}

	internal override void ProcessOneOperand(IVMNetworkAdapterOwner operand, IOperationWatcher operationWatcher)
	{
		if (CurrentParameterSetIs("ManagementOS"))
		{
			VMSwitch vMSwitch = operand as VMSwitch;
			if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_AddVMNetworkAdapter_VMSwitch, vMSwitch.Name)))
			{
				VMInternalNetworkAdapter output = vMSwitch.AddInternalNetworkAdapter(string.IsNullOrEmpty(Name) ? vMSwitch.Name : Name, StaticMacAddress, operationWatcher);
				if (Passthru.IsPresent)
				{
					operationWatcher.WriteObject(output);
				}
			}
			return;
		}
		VirtualMachine virtualMachine = operand as VirtualMachine;
		if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_AddVMNetworkAdapter_VM, virtualMachine.Name)))
		{
			VMNetworkAdapter vMNetworkAdapter = VMNetworkAdapter.CreateTemplateForAdd(IsParameterSpecified("IsLegacy") && IsLegacy, virtualMachine);
			ConfigureTemplateAdapter(vMNetworkAdapter);
			VMNetworkAdapter output2 = VMNetworkAdapter.ApplyAdd(vMNetworkAdapter, operationWatcher);
			if (Passthru.IsPresent)
			{
				operationWatcher.WriteObject(output2);
			}
		}
	}

	private void ConfigureTemplateAdapter(VMNetworkAdapter adapter)
	{
		adapter.DynamicMacAddressEnabled = DynamicMacAddress.IsPresent;
		adapter.MacAddress = StaticMacAddress ?? string.Empty;
		adapter.AllowPacketDirect = IsParameterSpecified("AllowPacketDirect") && (bool)AllowPacketDirect;
		adapter.NumaAwarePlacement = IsParameterSpecified("NumaAwarePlacement") && NumaAwarePlacement;
		if (IsParameterSpecified("DeviceNaming"))
		{
			adapter.DeviceNaming = DeviceNaming;
		}
		if (!string.IsNullOrEmpty(Name))
		{
			adapter.Name = Name;
		}
		if (ResourcePoolName != null)
		{
			adapter.PoolName = ResourcePoolName;
			adapter.IsEnabled = true;
		}
		if (!string.IsNullOrEmpty(SwitchName))
		{
			adapter.SetConnectedSwitchName(SwitchName);
			adapter.IsEnabled = true;
		}
	}
}

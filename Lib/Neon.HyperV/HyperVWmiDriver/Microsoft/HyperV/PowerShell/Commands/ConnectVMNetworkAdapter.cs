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

[Cmdlet("Connect", "VMNetworkAdapter", SupportsShouldProcess = true, DefaultParameterSetName = "Name_SwitchName")]
[OutputType(new Type[] { typeof(VMNetworkAdapter) })]
internal sealed class ConnectVMNetworkAdapter : VirtualizationCmdlet<VMNetworkAdapter>, IVmByVMNameCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, ISupportsPassthrough
{
	internal static class ParameterSetNames
	{
		public const string AdapterNameSwitchName = "Name_SwitchName";

		public const string AdapterNameSwitchObject = "Name_SwitchObject";

		public const string AdapterNameUseAutomaticConnection = "Name_UseAutomaticConnection";

		public const string AdapterObjectSwitchName = "Object_SwitchName";

		public const string AdapterObjectSwitchObject = "Object_SwitchObject";

		public const string AdapterObjectUseAutomaticConnection = "Object_UseAutomaticConnection";
	}

	private enum ConnectionParameterSetType
	{
		Unknown,
		SwitchName,
		SwitchObject,
		AutomaticConnection
	}

	private ConnectionParameterSetType ParameterSetType
	{
		get
		{
			if (CurrentParameterSetIs("Name_SwitchName") || CurrentParameterSetIs("Object_SwitchName"))
			{
				return ConnectionParameterSetType.SwitchName;
			}
			if (CurrentParameterSetIs("Name_SwitchObject") || CurrentParameterSetIs("Object_SwitchObject"))
			{
				return ConnectionParameterSetType.SwitchObject;
			}
			if (CurrentParameterSetIs("Name_UseAutomaticConnection") || CurrentParameterSetIs("Object_UseAutomaticConnection"))
			{
				return ConnectionParameterSetType.AutomaticConnection;
			}
			return ConnectionParameterSetType.Unknown;
		}
	}

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
	[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "Object_SwitchName")]
	[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "Object_SwitchObject")]
	[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "Object_UseAutomaticConnection")]
	[ValidateNotNullOrEmpty]
	public VMNetworkAdapter[] VMNetworkAdapter { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[ValidateNotNullOrEmpty]
	[Parameter(Position = 1, ParameterSetName = "Name_SwitchObject")]
	[Parameter(Position = 1, ParameterSetName = "Name_SwitchName")]
	[Parameter(Position = 1, ParameterSetName = "Name_UseAutomaticConnection")]
	[Alias(new string[] { "VMNetworkAdapterName" })]
	public string[] Name { get; set; }

	[Parameter(Mandatory = true, Position = 2, ParameterSetName = "Name_SwitchName")]
	[Parameter(Mandatory = true, Position = 1, ParameterSetName = "Object_SwitchName")]
	[ValidateNotNullOrEmpty]
	public string SwitchName { get; set; }

	[Parameter(Mandatory = true, Position = 2, ParameterSetName = "Name_SwitchObject", ValueFromPipeline = true)]
	[Parameter(Mandatory = true, Position = 1, ParameterSetName = "Object_SwitchObject", ValueFromPipeline = true)]
	[ValidateNotNull]
	public VMSwitch VMSwitch { get; set; }

	[Parameter(Mandatory = true, ParameterSetName = "Name_UseAutomaticConnection")]
	[Parameter(Mandatory = true, ParameterSetName = "Object_UseAutomaticConnection")]
	public SwitchParameter UseAutomaticConnection { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
	[Parameter]
	public SwitchParameter Passthru { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "Name_SwitchName")]
	[Parameter(ParameterSetName = "Name_UseAutomaticConnection")]
	[ValidateNotNullOrEmpty]
	public override CimSession[] CimSession { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "Name_SwitchName")]
	[Parameter(ParameterSetName = "Name_UseAutomaticConnection")]
	[ValidateNotNullOrEmpty]
	public override string[] ComputerName { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "Name_SwitchName")]
	[Parameter(ParameterSetName = "Name_UseAutomaticConnection")]
	[ValidateNotNullOrEmpty]
	[CredentialArray]
	public override PSCredential[] Credential { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true, Position = 0, ParameterSetName = "Name_SwitchObject")]
	[Parameter(Mandatory = true, Position = 0, ParameterSetName = "Name_SwitchName")]
	[Parameter(Mandatory = true, Position = 0, ParameterSetName = "Name_UseAutomaticConnection")]
	public string[] VMName { get; set; }

	internal override IList<VMNetworkAdapter> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		IList<VMNetworkAdapter> list;
		if (IsParameterSpecified("VMNetworkAdapter"))
		{
			list = VMNetworkAdapter;
		}
		else
		{
			IEnumerable<VMNetworkAdapter> source = ParameterResolvers.ResolveVirtualMachines(this, operationWatcher).SelectManyWithLogging((VirtualMachine vm) => vm.GetNetworkAdapters(), operationWatcher);
			if (Name.IsNullOrEmpty())
			{
				list = source.ToList();
			}
			else
			{
				WildcardPatternMatcher matcher = new WildcardPatternMatcher(Name);
				list = source.Where((VMNetworkAdapter adapter) => matcher.MatchesAny(adapter.Name)).ToList();
				if (list.Count == 0)
				{
					throw ExceptionHelper.CreateObjectNotFoundException(CmdletErrorMessages.VMNetworkAdapter_NoneFound, null);
				}
			}
		}
		return list;
	}

	internal override void ProcessOneOperand(VMNetworkAdapter networkAdapter, IOperationWatcher operationWatcher)
	{
		ConnectionParameterSetType parameterSetType = ParameterSetType;
		string description;
		if (parameterSetType == ConnectionParameterSetType.AutomaticConnection)
		{
			description = string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_ConnectVMNetworkAdapter_UseAutomaticConnection, networkAdapter.Name);
		}
		else
		{
			string arg = string.Empty;
			switch (parameterSetType)
			{
			case ConnectionParameterSetType.SwitchName:
				arg = SwitchName;
				break;
			case ConnectionParameterSetType.SwitchObject:
				arg = VMSwitch.Name;
				break;
			}
			description = string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_ConnectVMNetworkAdapter_ToSwitch, networkAdapter.Name, arg);
		}
		if (operationWatcher.ShouldProcess(description))
		{
			networkAdapter.PrepareForModify(operationWatcher);
			switch (parameterSetType)
			{
			case ConnectionParameterSetType.SwitchName:
				networkAdapter.SetConnectedSwitchName(SwitchName);
				break;
			case ConnectionParameterSetType.SwitchObject:
				networkAdapter.SetConnectedSwitch(VMSwitch);
				break;
			case ConnectionParameterSetType.AutomaticConnection:
				networkAdapter.SetConnectedSwitch(null);
				break;
			}
			((IUpdatable)networkAdapter).Put(operationWatcher);
			if (Passthru.IsPresent)
			{
				operationWatcher.WriteObject(networkAdapter);
			}
		}
	}
}

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

[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Fibre", Justification = "This is per spec.")]
[Cmdlet("Set", "VMFibreChannelHba", SupportsShouldProcess = true, DefaultParameterSetName = "VMName And Only SAN")]
[OutputType(new Type[] { typeof(VMFibreChannelHba) })]
internal sealed class SetVMFibreChannelHba : VirtualizationCmdlet<VMFibreChannelHba>, ISupportsPassthrough, IVmBySingularVMNameCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet
{
	internal static class ParameterSetNames
	{
		public const string VMNameManualWwn = "VMName And Manual WWN";

		public const string ObjectManualWwn = "Object And Manual WWN";

		public const string VMNameGenerateWwn = "VMName And Generate WWN";

		public const string ObjectGenerateWwn = "Object And Generate WWN";

		public const string VMNameOnlySan = "VMName And Only SAN";

		public const string ObjectOnlySan = "Object And Only SAN";
	}

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "VMName And Generate WWN")]
	[Parameter(ParameterSetName = "VMName And Manual WWN")]
	[Parameter(ParameterSetName = "VMName And Only SAN")]
	[ValidateNotNullOrEmpty]
	public override CimSession[] CimSession { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "VMName And Generate WWN")]
	[Parameter(ParameterSetName = "VMName And Manual WWN")]
	[Parameter(ParameterSetName = "VMName And Only SAN")]
	[ValidateNotNullOrEmpty]
	public override string[] ComputerName { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "VMName And Generate WWN")]
	[Parameter(ParameterSetName = "VMName And Manual WWN")]
	[Parameter(ParameterSetName = "VMName And Only SAN")]
	[ValidateNotNullOrEmpty]
	[CredentialArray]
	public override PSCredential[] Credential { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Fibre", Justification = "This is per spec.")]
	[Parameter(ParameterSetName = "Object And Generate WWN", Mandatory = true, Position = 0, ValueFromPipeline = true)]
	[Parameter(ParameterSetName = "Object And Manual WWN", Mandatory = true, Position = 0, ValueFromPipeline = true)]
	[Parameter(ParameterSetName = "Object And Only SAN", Mandatory = true, Position = 0, ValueFromPipeline = true)]
	[ValidateNotNullOrEmpty]
	public VMFibreChannelHba VMFibreChannelHba { get; set; }

	[Parameter(ParameterSetName = "VMName And Generate WWN", Mandatory = true, Position = 0)]
	[Parameter(ParameterSetName = "VMName And Manual WWN", Mandatory = true, Position = 0)]
	[Parameter(ParameterSetName = "VMName And Only SAN", Mandatory = true, Position = 0)]
	[ValidateNotNullOrEmpty]
	public string VMName { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "WorldWide", Justification = "This is per spec.")]
	[Parameter(ParameterSetName = "VMName And Generate WWN", Mandatory = true, Position = 1)]
	[Parameter(ParameterSetName = "VMName And Manual WWN", Mandatory = true, Position = 1)]
	[Parameter(ParameterSetName = "VMName And Only SAN", Mandatory = true, Position = 1)]
	[Alias(new string[] { "Wwnn1" })]
	[ValidateNotNullOrEmpty]
	public string WorldWideNodeNameSetA { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "WorldWide", Justification = "This is per spec.")]
	[Parameter(ParameterSetName = "VMName And Generate WWN", Mandatory = true, Position = 2)]
	[Parameter(ParameterSetName = "VMName And Manual WWN", Mandatory = true, Position = 2)]
	[Parameter(ParameterSetName = "VMName And Only SAN", Mandatory = true, Position = 2)]
	[Alias(new string[] { "Wwpn1" })]
	[ValidateNotNullOrEmpty]
	public string WorldWidePortNameSetA { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "WorldWide", Justification = "This is per spec.")]
	[Parameter(ParameterSetName = "VMName And Generate WWN", Mandatory = true, Position = 3)]
	[Parameter(ParameterSetName = "VMName And Manual WWN", Mandatory = true, Position = 3)]
	[Parameter(ParameterSetName = "VMName And Only SAN", Mandatory = true, Position = 3)]
	[Alias(new string[] { "Wwnn2" })]
	[ValidateNotNullOrEmpty]
	public string WorldWideNodeNameSetB { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "WorldWide", Justification = "This is per spec.")]
	[Parameter(ParameterSetName = "VMName And Generate WWN", Mandatory = true, Position = 4)]
	[Parameter(ParameterSetName = "VMName And Manual WWN", Mandatory = true, Position = 4)]
	[Parameter(ParameterSetName = "VMName And Only SAN", Mandatory = true, Position = 4)]
	[Alias(new string[] { "Wwpn2" })]
	[ValidateNotNullOrEmpty]
	public string WorldWidePortNameSetB { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "WWN", Justification = "This is per spec.")]
	[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "WWN", Justification = "This is by spec.")]
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Wwn")]
	[Parameter(ParameterSetName = "VMName And Generate WWN", Mandatory = true)]
	[Parameter(ParameterSetName = "Object And Generate WWN", Mandatory = true)]
	[ValidateNotNullOrEmpty]
	public SwitchParameter GenerateWwn { get; set; }

	[Parameter(ParameterSetName = "VMName And Only SAN", Mandatory = true)]
	[Parameter(ParameterSetName = "Object And Only SAN", Mandatory = true)]
	[ValidateNotNullOrEmpty]
	public string SanName { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "WorldWide", Justification = "This is per spec.")]
	[Parameter(ParameterSetName = "Object And Manual WWN")]
	[Parameter(ParameterSetName = "VMName And Manual WWN")]
	[ValidateNotNullOrEmpty]
	public string NewWorldWideNodeNameSetA { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "WorldWide", Justification = "This is per spec.")]
	[Parameter(ParameterSetName = "Object And Manual WWN")]
	[Parameter(ParameterSetName = "VMName And Manual WWN")]
	[ValidateNotNullOrEmpty]
	public string NewWorldWidePortNameSetA { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "WorldWide", Justification = "This is per spec.")]
	[Parameter(ParameterSetName = "Object And Manual WWN")]
	[Parameter(ParameterSetName = "VMName And Manual WWN")]
	[ValidateNotNullOrEmpty]
	public string NewWorldWideNodeNameSetB { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "WorldWide", Justification = "This is per spec.")]
	[Parameter(ParameterSetName = "Object And Manual WWN")]
	[Parameter(ParameterSetName = "VMName And Manual WWN")]
	[ValidateNotNullOrEmpty]
	public string NewWorldWidePortNameSetB { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
	[Parameter]
	public SwitchParameter Passthru { get; set; }

	protected override void ValidateParameters()
	{
		base.ValidateParameters();
		if (!CurrentParameterSetIs("VMName And Manual WWN") && !CurrentParameterSetIs("Object And Manual WWN"))
		{
			return;
		}
		string[] array = new string[4] { NewWorldWideNodeNameSetA, NewWorldWidePortNameSetA, NewWorldWideNodeNameSetB, NewWorldWidePortNameSetB };
		foreach (string text in array)
		{
			if (!string.IsNullOrEmpty(text))
			{
				VMFibreChannelHba.ValidateWorldWideName(text);
			}
		}
	}

	internal override IList<VMFibreChannelHba> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		if (VMFibreChannelHba != null)
		{
			return new List<VMFibreChannelHba> { VMFibreChannelHba };
		}
		return (from hba in ParameterResolvers.ResolveVirtualMachines(this, operationWatcher).SelectManyWithLogging((VirtualMachine vm) => vm.FibreChannelHostBusAdapters, operationWatcher)
			where hba.Matches(WorldWideNodeNameSetA, WorldWidePortNameSetA, WorldWideNodeNameSetB, WorldWidePortNameSetB)
			select hba).ToList();
	}

	internal override void ValidateOperandList(IList<VMFibreChannelHba> operands, IOperationWatcher operationWatcher)
	{
		base.ValidateOperandList(operands, operationWatcher);
		if (operands.Count <= 0 && (string.IsNullOrEmpty(VMName) || !WildcardPattern.ContainsWildcardCharacters(VMName)))
		{
			throw ExceptionHelper.CreateObjectNotFoundException(CmdletErrorMessages.VMFibreChannelHba_NoVirtualHbaFound, null);
		}
		if (operands.Count > 1)
		{
			throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.VMFibreChannelHba_MultipleHbasFoundMatchingCriteria);
		}
	}

	internal override void ProcessOneOperand(VMFibreChannelHba adapter, IOperationWatcher operationWatcher)
	{
		if (!operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_SetVMFibreChannelHba, adapter.Name)))
		{
			return;
		}
		if (!string.IsNullOrEmpty(SanName))
		{
			adapter.SanName = SanName;
		}
		else
		{
			string portNameA;
			string portNameB;
			string text;
			string text2;
			if (GenerateWwn.IsPresent)
			{
				VMFibreChannelHba.GenerateWorldWideNames(adapter.Server, out var nodeName, out portNameA, out portNameB);
				text = nodeName;
				text2 = nodeName;
			}
			else
			{
				text = NewWorldWideNodeNameSetA ?? adapter.WorldWideNodeNameSetA;
				text2 = NewWorldWideNodeNameSetB ?? adapter.WorldWideNodeNameSetB;
				portNameA = NewWorldWidePortNameSetA ?? adapter.WorldWidePortNameSetA;
				portNameB = NewWorldWidePortNameSetB ?? adapter.WorldWidePortNameSetB;
			}
			if (string.Equals(text, text2, StringComparison.OrdinalIgnoreCase) && string.Equals(portNameA, portNameB, StringComparison.OrdinalIgnoreCase))
			{
				throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.VMFibreChannelHba_WorldWideNameDuplicateSetsError);
			}
			adapter.WorldWideNodeNameSetA = text;
			adapter.WorldWidePortNameSetA = portNameA;
			adapter.WorldWideNodeNameSetB = text2;
			adapter.WorldWidePortNameSetB = portNameB;
		}
		((IUpdatable)adapter).Put(operationWatcher);
		if (Passthru.IsPresent)
		{
			operationWatcher.WriteObject(adapter);
		}
	}
}

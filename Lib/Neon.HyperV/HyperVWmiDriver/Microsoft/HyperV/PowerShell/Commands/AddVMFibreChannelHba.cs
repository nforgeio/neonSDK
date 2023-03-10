using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Fibre", Justification = "This is per spec.")]
[Cmdlet("Add", "VMFibreChannelHba", SupportsShouldProcess = true, DefaultParameterSetName = "VMName and GenerateWwn")]
[OutputType(new Type[] { typeof(VMFibreChannelHba) })]
internal sealed class AddVMFibreChannelHba : VirtualizationCmdlet<VirtualMachine>, ISupportsPassthrough, IVmByObjectCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmBySingularVMNameCmdlet
{
	internal static class FcParameterSetNames
	{
		public const string VmObjectAndGenerateWwn = "VM object and GenerateWwn";

		public const string VmNameAndGenerateWwn = "VMName and GenerateWwn";

		public const string VmObjectAndManualWwn = "VM Object and manual WWN";

		public const string VmNameAndManualWwn = "VMName and manual WWN";
	}

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "VMName and GenerateWwn")]
	[Parameter(ParameterSetName = "VMName and manual WWN")]
	[ValidateNotNullOrEmpty]
	public override CimSession[] CimSession { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "VMName and GenerateWwn")]
	[Parameter(ParameterSetName = "VMName and manual WWN")]
	[ValidateNotNullOrEmpty]
	public override string[] ComputerName { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "VMName and GenerateWwn")]
	[Parameter(ParameterSetName = "VMName and manual WWN")]
	[ValidateNotNullOrEmpty]
	[CredentialArray]
	public override PSCredential[] Credential { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "VM object and GenerateWwn", ValueFromPipeline = true, Position = 0, Mandatory = true)]
	[Parameter(ParameterSetName = "VM Object and manual WWN", ValueFromPipeline = true, Position = 0, Mandatory = true)]
	public VirtualMachine[] VM { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "VMName and GenerateWwn", ValueFromPipeline = true, Position = 0, Mandatory = true)]
	[Parameter(ParameterSetName = "VMName and manual WWN", ValueFromPipeline = true, Position = 0, Mandatory = true)]
	public string VMName { get; set; }

	[Parameter(Mandatory = true, Position = 1)]
	[ValidateNotNullOrEmpty]
	public string SanName { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "WWN", Justification = "This is per spec.")]
	[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "WWN", Justification = "This is by spec.")]
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Wwn")]
	[Parameter(ParameterSetName = "VMName and GenerateWwn")]
	[Parameter(ParameterSetName = "VM object and GenerateWwn")]
	public SwitchParameter GenerateWwn { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "WorldWide", Justification = "This is per spec.")]
	[Parameter(Mandatory = true, ParameterSetName = "VMName and manual WWN")]
	[Parameter(Mandatory = true, ParameterSetName = "VM Object and manual WWN")]
	[ValidateNotNullOrEmpty]
	[Alias(new string[] { "Wwnn1" })]
	public string WorldWideNodeNameSetA { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "WorldWide", Justification = "This is per spec.")]
	[Parameter(Mandatory = true, ParameterSetName = "VMName and manual WWN")]
	[Parameter(Mandatory = true, ParameterSetName = "VM Object and manual WWN")]
	[ValidateNotNullOrEmpty]
	[Alias(new string[] { "Wwpn1" })]
	public string WorldWidePortNameSetA { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "WorldWide", Justification = "This is per spec.")]
	[Parameter(Mandatory = true, ParameterSetName = "VMName and manual WWN")]
	[Parameter(Mandatory = true, ParameterSetName = "VM Object and manual WWN")]
	[ValidateNotNullOrEmpty]
	[Alias(new string[] { "Wwnn2" })]
	public string WorldWideNodeNameSetB { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "WorldWide", Justification = "This is per spec.")]
	[Parameter(Mandatory = true, ParameterSetName = "VMName and manual WWN")]
	[Parameter(Mandatory = true, ParameterSetName = "VM Object and manual WWN")]
	[ValidateNotNullOrEmpty]
	[Alias(new string[] { "Wwpn2" })]
	public string WorldWidePortNameSetB { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
	[Parameter]
	public SwitchParameter Passthru { get; set; }

	protected override void ValidateParameters()
	{
		base.ValidateParameters();
		if (!CurrentParameterSetIs("VMName and manual WWN") && !CurrentParameterSetIs("VM Object and manual WWN"))
		{
			return;
		}
		string[] array = new string[4] { WorldWideNodeNameSetA, WorldWidePortNameSetA, WorldWideNodeNameSetB, WorldWidePortNameSetB };
		foreach (string text in array)
		{
			if (!string.IsNullOrEmpty(text))
			{
				VMFibreChannelHba.ValidateWorldWideName(text);
			}
		}
		if (string.Equals(WorldWideNodeNameSetA, WorldWideNodeNameSetB, StringComparison.OrdinalIgnoreCase) && string.Equals(WorldWidePortNameSetA, WorldWidePortNameSetB, StringComparison.OrdinalIgnoreCase))
		{
			throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.VMFibreChannelHba_WorldWideNameDuplicateSetsError);
		}
	}

	internal override IList<VirtualMachine> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		return ParameterResolvers.ResolveVirtualMachines(this, operationWatcher);
	}

	internal override void ProcessOneOperand(VirtualMachine operand, IOperationWatcher operationWatcher)
	{
		if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_AddVMFibreChannelHba, operand.Name)))
		{
			VMFibreChannelHba vMFibreChannelHba = VMFibreChannelHba.CreateTemplateFibreChannelHba(operand);
			if (CurrentParameterSetIs("VMName and manual WWN") || CurrentParameterSetIs("VM Object and manual WWN"))
			{
				vMFibreChannelHba.WorldWideNodeNameSetA = WorldWideNodeNameSetA;
				vMFibreChannelHba.WorldWidePortNameSetA = WorldWidePortNameSetA;
				vMFibreChannelHba.WorldWideNodeNameSetB = WorldWideNodeNameSetB;
				vMFibreChannelHba.WorldWidePortNameSetB = WorldWidePortNameSetB;
			}
			else
			{
				VMFibreChannelHba.GenerateWorldWideNames(operand.Server, out var nodeName, out var portNameA, out var portNameB);
				vMFibreChannelHba.WorldWideNodeNameSetA = nodeName;
				vMFibreChannelHba.WorldWidePortNameSetA = portNameA;
				vMFibreChannelHba.WorldWideNodeNameSetB = nodeName;
				vMFibreChannelHba.WorldWidePortNameSetB = portNameB;
			}
			if (!string.IsNullOrEmpty(SanName))
			{
				vMFibreChannelHba.SanName = SanName;
			}
			VMFibreChannelHba output = operand.AddFibreChannelAdapter(vMFibreChannelHba, operationWatcher);
			if (Passthru.IsPresent)
			{
				operationWatcher.WriteObject(output);
			}
		}
	}
}

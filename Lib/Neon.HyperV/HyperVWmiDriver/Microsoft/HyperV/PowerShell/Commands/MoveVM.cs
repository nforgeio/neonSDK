using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.Management.Infrastructure;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Move", "VM", DefaultParameterSetName = "NameSingleDestination", SupportsShouldProcess = true)]
[OutputType(new Type[] { typeof(VirtualMachine) })]
internal sealed class MoveVM : VirtualizationCmdlet<VirtualMachine>, ISupportsPassthrough, ISupportsAsJob, IMoveOrCompareVMCmdlet, IVMSingularObjectOrNameCmdlet, IVmBySingularObjectCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmBySingularNameCmdlet, IDestinationServerParameters
{
	private IReadOnlyList<VhdMigrationMapping> m_VhdMigrationMappings;

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "NameSingleDestinationAndCimSession")]
	[Parameter(ParameterSetName = "NameMultipleDestinationsAndCimSession")]
	public override CimSession[] CimSession { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "NameSingleDestination")]
	[Parameter(ParameterSetName = "NameMultipleDestinations")]
	public override string[] ComputerName { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "NameSingleDestination")]
	[Parameter(ParameterSetName = "NameMultipleDestinations")]
	[CredentialArray]
	public override PSCredential[] Credential { get; set; }

	[Parameter(Mandatory = true, ParameterSetName = "CompatibilityReport", ValueFromPipeline = true, Position = 0)]
	public VMCompatibilityReport CompatibilityReport { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "NameSingleDestination")]
	[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "NameSingleDestinationAndCimSession")]
	[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "NameMultipleDestinations")]
	[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "NameMultipleDestinationsAndCimSession")]
	public string Name { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "VMSingleDestination")]
	[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "VMSingleDestinationAndCimSession")]
	[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "VMMultipleDestinations")]
	[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "VMMultipleDestinationsAndCimSession")]
	public VirtualMachine VM { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true, Position = 1, ParameterSetName = "NameSingleDestinationAndCimSession")]
	[Parameter(Mandatory = true, Position = 1, ParameterSetName = "NameMultipleDestinationsAndCimSession")]
	[Parameter(Mandatory = true, Position = 1, ParameterSetName = "VMSingleDestinationAndCimSession")]
	[Parameter(Mandatory = true, Position = 1, ParameterSetName = "VMMultipleDestinationsAndCimSession")]
	public CimSession DestinationCimSession { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true, Position = 1, ParameterSetName = "NameSingleDestination")]
	[Parameter(Mandatory = true, Position = 1, ParameterSetName = "NameMultipleDestinations")]
	[Parameter(Mandatory = true, Position = 1, ParameterSetName = "VMSingleDestination")]
	[Parameter(Mandatory = true, Position = 1, ParameterSetName = "VMMultipleDestinations")]
	public string DestinationHost { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "NameSingleDestination")]
	[Parameter(ParameterSetName = "NameMultipleDestinations")]
	[Parameter(ParameterSetName = "VMSingleDestination")]
	[Parameter(ParameterSetName = "VMMultipleDestinations")]
	[Credential]
	public PSCredential DestinationCredential { get; set; }

	[Parameter(ParameterSetName = "NameSingleDestination")]
	[Parameter(ParameterSetName = "NameSingleDestinationAndCimSession")]
	[Parameter(ParameterSetName = "VMSingleDestination")]
	[Parameter(ParameterSetName = "VMSingleDestinationAndCimSession")]
	public SwitchParameter IncludeStorage { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "NameSingleDestination")]
	[Parameter(ParameterSetName = "NameSingleDestinationAndCimSession")]
	[Parameter(ParameterSetName = "VMSingleDestination")]
	[Parameter(ParameterSetName = "VMSingleDestinationAndCimSession")]
	public string DestinationStoragePath { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "NameMultipleDestinations")]
	[Parameter(Mandatory = true, ParameterSetName = "NameMultipleDestinationsAndCimSession")]
	[Parameter(ParameterSetName = "VMMultipleDestinations")]
	[Parameter(Mandatory = true, ParameterSetName = "VMMultipleDestinationsAndCimSession")]
	public string VirtualMachinePath { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "NameMultipleDestinations")]
	[Parameter(ParameterSetName = "NameMultipleDestinationsAndCimSession")]
	[Parameter(ParameterSetName = "VMMultipleDestinations")]
	[Parameter(ParameterSetName = "VMMultipleDestinationsAndCimSession")]
	[Alias(new string[] { "CheckpointFileLocation", "SnapshotFileLocation" })]
	public string SnapshotFilePath { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "NameMultipleDestinations")]
	[Parameter(ParameterSetName = "NameMultipleDestinationsAndCimSession")]
	[Parameter(ParameterSetName = "VMMultipleDestinations")]
	[Parameter(ParameterSetName = "VMMultipleDestinationsAndCimSession")]
	public string SmartPagingFilePath { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Vhds", Justification = "This is by spec.")]
	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "NameMultipleDestinations")]
	[Parameter(ParameterSetName = "NameMultipleDestinationsAndCimSession")]
	[Parameter(ParameterSetName = "VMMultipleDestinations")]
	[Parameter(ParameterSetName = "VMMultipleDestinationsAndCimSession")]
	public Hashtable[] Vhds { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "NameSingleDestination")]
	[Parameter(ParameterSetName = "NameSingleDestinationAndCimSession")]
	[Parameter(ParameterSetName = "NameMultipleDestinations")]
	[Parameter(ParameterSetName = "NameMultipleDestinationsAndCimSession")]
	[Parameter(ParameterSetName = "VMSingleDestination")]
	[Parameter(ParameterSetName = "VMSingleDestinationAndCimSession")]
	[Parameter(ParameterSetName = "VMMultipleDestinations")]
	[Parameter(ParameterSetName = "VMMultipleDestinationsAndCimSession")]
	public string ResourcePoolName { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "VHDs", Justification = "This is per spec.")]
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Vhd", Justification = "This is a standard PowerShell term.")]
	[Parameter(ParameterSetName = "NameSingleDestination")]
	[Parameter(ParameterSetName = "NameSingleDestinationAndCimSession")]
	[Parameter(ParameterSetName = "NameMultipleDestinations")]
	[Parameter(ParameterSetName = "NameMultipleDestinationsAndCimSession")]
	[Parameter(ParameterSetName = "VMSingleDestination")]
	[Parameter(ParameterSetName = "VMSingleDestinationAndCimSession")]
	[Parameter(ParameterSetName = "VMMultipleDestinations")]
	[Parameter(ParameterSetName = "VMMultipleDestinationsAndCimSession")]
	public SwitchParameter RetainVhdCopiesOnSource { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "VHDs", Justification = "This is per spec.")]
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Vhd", Justification = "This is a standard PowerShell term.")]
	[Parameter(ParameterSetName = "NameSingleDestination")]
	[Parameter(ParameterSetName = "NameSingleDestinationAndCimSession")]
	[Parameter(ParameterSetName = "NameMultipleDestinations")]
	[Parameter(ParameterSetName = "NameMultipleDestinationsAndCimSession")]
	[Parameter(ParameterSetName = "VMSingleDestination")]
	[Parameter(ParameterSetName = "VMSingleDestinationAndCimSession")]
	[Parameter(ParameterSetName = "VMMultipleDestinations")]
	[Parameter(ParameterSetName = "VMMultipleDestinationsAndCimSession")]
	public SwitchParameter RemoveSourceUnmanagedVhds { get; set; }

	[Parameter]
	public SwitchParameter AsJob { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
	[Parameter]
	public SwitchParameter Passthru { get; set; }

	IReadOnlyList<VhdMigrationMapping> IMoveOrCompareVMCmdlet.VhdMigrationMappings
	{
		get
		{
			return m_VhdMigrationMappings;
		}
		set
		{
			m_VhdMigrationMappings = value;
		}
	}

	public override VirtualMachineParameterType VirtualMachineParameterType => ParameterResolvers.GetVirtualMachineParameterType(this);

	protected override void NormalizeParameters()
	{
		base.NormalizeParameters();
		CurrentParameterSetIs("CompatibilityReport");
		ParameterNormalizers.NormalizeMoveOrCompareVMParameters(this, base.CurrentFileSystemLocation);
	}

	protected override void ValidateParameters()
	{
		base.ValidateParameters();
		if (!CurrentParameterSetIs("CompatibilityReport"))
		{
			ParameterValidator.ValidateDestinationServerParameters(this);
			ParameterValidator.ValidateMoveOrCompareVMParameters(this, IncludeStorage.IsPresent);
		}
	}

	internal override IList<VirtualMachine> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		if (!CurrentParameterSetIs("CompatibilityReport"))
		{
			return ParameterResolvers.ResolveVirtualMachines(this, operationWatcher);
		}
		return VirtualizationObjectLocator.GetVirtualMachinesByIdsAndServers(ParameterResolvers.GetServers(this, operationWatcher), new List<Guid> { CompatibilityReport.VM.Id }, ErrorDisplayMode.WriteError, operationWatcher);
	}

	internal override void ProcessOneOperand(VirtualMachine operand, IOperationWatcher operationWatcher)
	{
		Server server = ((CompatibilityReport == null) ? ParameterResolvers.GetDestinationServer(this) : CompatibilityReport.VM.Server);
		if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_MoveVM, operand.Name, server)))
		{
			if (CurrentParameterSetIs("CompatibilityReport"))
			{
				operand.MoveTo(CompatibilityReport, RetainVhdCopiesOnSource.IsPresent, RemoveSourceUnmanagedVhds.IsPresent, operationWatcher);
			}
			else if (ParameterResolvers.IsMovingToSingleLocation(this))
			{
				operand.MoveTo(server, IncludeStorage.IsPresent, DestinationStoragePath, ResourcePoolName, RetainVhdCopiesOnSource.IsPresent, RemoveSourceUnmanagedVhds.IsPresent, operationWatcher);
			}
			else
			{
				operand.MoveTo(server, VirtualMachinePath, SnapshotFilePath, SmartPagingFilePath, m_VhdMigrationMappings, ResourcePoolName, RetainVhdCopiesOnSource.IsPresent, RemoveSourceUnmanagedVhds.IsPresent, operationWatcher);
			}
			if (Passthru.IsPresent)
			{
				VirtualMachine virtualMachineById = VirtualizationObjectLocator.GetVirtualMachineById(server, operand.Id);
				operationWatcher.WriteObject(virtualMachineById);
			}
		}
	}
}

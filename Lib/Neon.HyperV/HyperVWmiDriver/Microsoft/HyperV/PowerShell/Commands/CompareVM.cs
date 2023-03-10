using System;
using System.Collections;
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

[Cmdlet("Compare", "VM", DefaultParameterSetName = "NameSingleDestination", SupportsShouldProcess = true)]
[OutputType(new Type[] { typeof(VMCompatibilityReport) })]
internal sealed class CompareVM : VirtualizationCreationCmdlet<VMCompatibilityReport>, IMoveOrCompareVMCmdlet, IVMSingularObjectOrNameCmdlet, IVmBySingularObjectCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmBySingularNameCmdlet, IImportOrCompareVMCmdlet, IDestinationServerParameters, ISupportsAsJob
{
	private IReadOnlyList<VhdMigrationMapping> m_VhdMigrationMappings;

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "NameSingleDestinationAndCimSession")]
	[Parameter(ParameterSetName = "NameMultipleDestinationsAndCimSession")]
	[Parameter(ParameterSetName = "Register")]
	[Parameter(ParameterSetName = "Copy")]
	[ValidateNotNullOrEmpty]
	public override CimSession[] CimSession { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "NameSingleDestination")]
	[Parameter(ParameterSetName = "NameMultipleDestinations")]
	[Parameter(ParameterSetName = "Register")]
	[Parameter(ParameterSetName = "Copy")]
	[ValidateNotNullOrEmpty]
	public override string[] ComputerName { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "NameSingleDestination")]
	[Parameter(ParameterSetName = "NameMultipleDestinations")]
	[Parameter(ParameterSetName = "Register")]
	[Parameter(ParameterSetName = "Copy")]
	[ValidateNotNullOrEmpty]
	[CredentialArray]
	public override PSCredential[] Credential { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "NameMultipleDestinations")]
	[Parameter(Mandatory = true, ParameterSetName = "NameMultipleDestinationsAndCimSession")]
	[Parameter(ParameterSetName = "VMMultipleDestinations")]
	[Parameter(Mandatory = true, ParameterSetName = "VMMultipleDestinationsAndCimSession")]
	[Parameter(ParameterSetName = "Copy")]
	public string VirtualMachinePath { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "NameMultipleDestinations")]
	[Parameter(ParameterSetName = "NameMultipleDestinationsAndCimSession")]
	[Parameter(ParameterSetName = "VMMultipleDestinations")]
	[Parameter(ParameterSetName = "VMMultipleDestinationsAndCimSession")]
	[Parameter(ParameterSetName = "Copy")]
	[Alias(new string[] { "CheckpointFileLocation", "SnapshotFileLocation" })]
	public string SnapshotFilePath { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "NameMultipleDestinations")]
	[Parameter(ParameterSetName = "NameMultipleDestinationsAndCimSession")]
	[Parameter(ParameterSetName = "VMMultipleDestinations")]
	[Parameter(ParameterSetName = "VMMultipleDestinationsAndCimSession")]
	[Parameter(ParameterSetName = "Copy")]
	public string SmartPagingFilePath { get; set; }

	[Parameter(Mandatory = true, ParameterSetName = "CompatibilityReport", ValueFromPipeline = true, Position = 0)]
	public VMCompatibilityReport CompatibilityReport { get; set; }

	[Parameter]
	public SwitchParameter AsJob { get; set; }

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

	[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "Register")]
	[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "Copy")]
	[ValidateNotNullOrEmpty]
	public string Path { get; set; }

	[Parameter(Mandatory = false, ParameterSetName = "Copy", Position = 1)]
	public string VhdDestinationPath { get; set; }

	[Parameter(ParameterSetName = "Register")]
	public SwitchParameter Register { get; set; }

	[Parameter(Mandatory = true, ParameterSetName = "Copy")]
	public SwitchParameter Copy { get; set; }

	[Parameter(ParameterSetName = "Copy")]
	public string VhdSourcePath { get; set; }

	[Parameter(ParameterSetName = "Copy")]
	public SwitchParameter GenerateNewId { get; set; }

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
		string currentFileSystemLocation = base.CurrentFileSystemLocation;
		if (VirtualMachineParameterType != 0)
		{
			ParameterNormalizers.NormalizeMoveOrCompareVMParameters(this, currentFileSystemLocation);
		}
		else if (!CurrentParameterSetIs("CompatibilityReport"))
		{
			ParameterNormalizers.NormalizeImportOrCompareVMParameters(this, currentFileSystemLocation);
		}
		base.NormalizeParameters();
	}

	protected override void ValidateParameters()
	{
		base.ValidateParameters();
		if (VirtualMachineParameterType != 0)
		{
			ParameterValidator.ValidateDestinationServerParameters(this);
			ParameterValidator.ValidateMoveOrCompareVMParameters(this, IncludeStorage.IsPresent);
		}
		else if (!CurrentParameterSetIs("CompatibilityReport"))
		{
			ParameterValidator.ValidateImportOrCompareVMParameters(this);
		}
	}

	internal override IList<VMCompatibilityReport> CreateObjects(IOperationWatcher operationWatcher)
	{
		List<VMCompatibilityReport> list = new List<VMCompatibilityReport>();
		if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_CompareVM)))
		{
			if (VirtualMachineParameterType == VirtualMachineParameterType.SingularName || VirtualMachineParameterType == VirtualMachineParameterType.SingularVMObject)
			{
				foreach (VirtualMachine item2 in ParameterResolvers.ResolveVirtualMachines(this, operationWatcher))
				{
					Server destinationServer = ParameterResolvers.GetDestinationServer(this);
					VMCompatibilityReport item = ((!ParameterResolvers.IsMovingToSingleLocation(this)) ? item2.CheckCompatibility(destinationServer, VirtualMachinePath, SnapshotFilePath, VirtualMachinePath, m_VhdMigrationMappings, ResourcePoolName, RetainVhdCopiesOnSource, operationWatcher) : item2.CheckCompatibility(destinationServer, IncludeStorage.IsPresent, DestinationStoragePath, ResourcePoolName, RetainVhdCopiesOnSource, operationWatcher));
					list.Add(item);
				}
				return list;
			}
			if (CurrentParameterSetIs("Register") || CurrentParameterSetIs("Copy"))
			{
				list = ParameterResolvers.GenerateCompatibilityReports(this, operationWatcher).ToList();
			}
			else
			{
				VMCompatibilityReport oldReport = CompatibilityReport;
				switch (CompatibilityReport.OperationType)
				{
				case OperationType.ImportVirtualMachine:
					list.Add(VMCompatibilityReport.RegenerateReportForImport(oldReport, operationWatcher));
					break;
				case OperationType.MoveVirtualMachine:
				case OperationType.MoveVirtualMachineAndStorage:
					list.AddRange(VirtualizationObjectLocator.GetVirtualMachinesByIdsAndServers(ParameterResolvers.GetServers(this, operationWatcher), new Guid[1] { oldReport.VM.Id }, ErrorDisplayMode.WriteError, operationWatcher).SelectWithLogging((VirtualMachine virtualMachine) => virtualMachine.CheckCompatibility(oldReport, RetainVhdCopiesOnSource.IsPresent, removeSourceUnmanagedVhds: false, operationWatcher), operationWatcher));
					break;
				default:
					throw new NotSupportedException();
				}
			}
		}
		return list;
	}
}

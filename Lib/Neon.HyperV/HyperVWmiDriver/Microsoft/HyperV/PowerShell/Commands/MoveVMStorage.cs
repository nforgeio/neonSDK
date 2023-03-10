using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Move", "VMStorage", DefaultParameterSetName = "NameSingleDestination", SupportsShouldProcess = true)]
internal sealed class MoveVMStorage : VirtualizationCmdlet<VirtualMachine>, ISupportsAsJob, IMoveOrCompareVMCmdlet, IVMSingularObjectOrNameCmdlet, IVmBySingularObjectCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmBySingularNameCmdlet
{
	private IReadOnlyList<VhdMigrationMapping> m_VhdMigrationMappings;

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "NameSingleDestination")]
	[Parameter(ParameterSetName = "NameMultipleDestinations")]
	[ValidateNotNullOrEmpty]
	public override CimSession[] CimSession { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "NameSingleDestination")]
	[Parameter(ParameterSetName = "NameMultipleDestinations")]
	[ValidateNotNullOrEmpty]
	public override string[] ComputerName { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "NameSingleDestination")]
	[Parameter(ParameterSetName = "NameMultipleDestinations")]
	[ValidateNotNullOrEmpty]
	[CredentialArray]
	public override PSCredential[] Credential { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "NameSingleDestination")]
	[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "NameMultipleDestinations")]
	[Alias(new string[] { "VMName" })]
	public string Name { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "VMSingleDestination")]
	[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "VMMultipleDestinations")]
	public VirtualMachine VM { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true, Position = 1, ParameterSetName = "VMSingleDestination")]
	[Parameter(Mandatory = true, Position = 1, ParameterSetName = "NameSingleDestination")]
	public string DestinationStoragePath { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "NameMultipleDestinations")]
	[Parameter(ParameterSetName = "VMMultipleDestinations")]
	public string VirtualMachinePath { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "NameMultipleDestinations")]
	[Parameter(ParameterSetName = "VMMultipleDestinations")]
	[Alias(new string[] { "CheckpointFileLocation", "SnapshotFileLocation" })]
	public string SnapshotFilePath { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "NameMultipleDestinations")]
	[Parameter(ParameterSetName = "VMMultipleDestinations")]
	public string SmartPagingFilePath { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Vhds", Justification = "This is by spec.")]
	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "NameMultipleDestinations")]
	[Parameter(ParameterSetName = "VMMultipleDestinations")]
	public Hashtable[] Vhds { get; set; }

	[Parameter]
	[ValidateNotNullOrEmpty]
	public string ResourcePoolName { get; set; }

	[Parameter]
	[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "VHDs", Justification = "This is per spec.")]
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Vhd", Justification = "This is a standard PowerShell term.")]
	public SwitchParameter RetainVhdCopiesOnSource { get; set; }

	[Parameter]
	[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "VHDs", Justification = "This is per spec.")]
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Vhd", Justification = "This is a standard PowerShell term.")]
	public SwitchParameter RemoveSourceUnmanagedVhds { get; set; }

	[Parameter]
	public SwitchParameter AllowUnverifiedPaths { get; set; }

	[Parameter]
	public SwitchParameter AsJob { get; set; }

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
		ParameterNormalizers.NormalizeMoveOrCompareVMParameters(this, base.CurrentFileSystemLocation);
		if (Vhds != null)
		{
			m_VhdMigrationMappings = VhdMigrationMapping.CreateMappingsFromHashtable(Vhds);
		}
	}

	protected override void ValidateParameters()
	{
		base.ValidateParameters();
		ParameterValidator.ValidateMoveOrCompareVMParameters(this, includeStorage: true);
	}

	internal override IList<VirtualMachine> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		return ParameterResolvers.ResolveVirtualMachines(this, operationWatcher);
	}

	internal override void ProcessOneOperand(VirtualMachine operand, IOperationWatcher operationWatcher)
	{
		if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_MoveVMStorage, operand.Name)))
		{
			bool isClustered = operand.IsClustered;
			if (ParameterResolvers.IsMovingToSingleLocation(this))
			{
				operand.MoveStorage(DestinationStoragePath, ResourcePoolName, RetainVhdCopiesOnSource.IsPresent, RemoveSourceUnmanagedVhds.IsPresent, AllowUnverifiedPaths.IsPresent, operationWatcher);
			}
			else
			{
				operand.MoveStorage(VirtualMachinePath, SnapshotFilePath, SmartPagingFilePath, m_VhdMigrationMappings, ResourcePoolName, RetainVhdCopiesOnSource.IsPresent, RemoveSourceUnmanagedVhds.IsPresent, AllowUnverifiedPaths.IsPresent, operationWatcher);
			}
			if (isClustered)
			{
				ClusterUtilities.UpdateClusterVMConfiguration(operand, base.InvokeCommand, operationWatcher);
			}
		}
	}
}

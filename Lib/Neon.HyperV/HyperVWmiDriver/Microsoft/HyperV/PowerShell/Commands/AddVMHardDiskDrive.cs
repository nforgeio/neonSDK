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

[Cmdlet("Add", "VMHardDiskDrive", SupportsShouldProcess = true, DefaultParameterSetName = "VMName")]
[OutputType(new Type[] { typeof(HardDiskDrive) })]
internal sealed class AddVMHardDiskDrive : VirtualizationCmdlet<Tuple<VMDriveController, int>>, IVMObjectOrVMNameCmdlet, IVmByObjectCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmByVMNameCmdlet, ISupportsPassthrough
{
	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "VMName")]
	[ValidateNotNullOrEmpty]
	public override CimSession[] CimSession { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "VMName")]
	[Alias(new string[] { "PSComputerName" })]
	[ValidateNotNullOrEmpty]
	public override string[] ComputerName { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "VMName")]
	[ValidateNotNullOrEmpty]
	[CredentialArray]
	public override PSCredential[] Credential { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "VMName", ValueFromPipeline = true, Position = 0, Mandatory = true)]
	public string[] VMName { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "VMObject", ValueFromPipeline = true, Position = 0, Mandatory = true)]
	public VirtualMachine[] VM { get; set; }

	[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "VMDriveController")]
	public VMDriveController VMDriveController { get; set; }

	[Parameter(Position = 1, ParameterSetName = "VMObject")]
	[Parameter(Position = 1, ParameterSetName = "VMName")]
	public ControllerType? ControllerType { get; set; }

	[ValidateNotNull]
	[Parameter(Position = 2, ParameterSetName = "VMObject")]
	[Parameter(Position = 2, ParameterSetName = "VMName")]
	public int? ControllerNumber { get; set; }

	[ValidateNotNull]
	[Parameter(Position = 3)]
	public int? ControllerLocation { get; set; }

	[Parameter(Position = 4)]
	[ValidateNotNullOrEmpty]
	public string Path { get; set; }

	[Alias(new string[] { "Number" })]
	[Parameter(ValueFromPipelineByPropertyName = true)]
	public uint DiskNumber { get; set; }

	[ValidateNotNull]
	[Parameter]
	public string ResourcePoolName { get; set; }

	[Parameter]
	[Alias(new string[] { "ShareVirtualDisk" })]
	public SwitchParameter SupportPersistentReservations { get; set; }

	[Parameter]
	public SwitchParameter AllowUnverifiedPaths { get; set; }

	[Parameter]
	public ulong? MaximumIOPS { get; set; }

	[Parameter]
	public ulong? MinimumIOPS { get; set; }

	[Parameter]
	public string QoSPolicyID { get; set; }

	[Parameter]
	public CimInstance QoSPolicy { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
	[Parameter]
	public SwitchParameter Passthru { get; set; }

	[Parameter]
	public CacheAttributes? OverrideCacheAttributes { get; set; }

	protected override void NormalizeParameters()
	{
		if (!string.IsNullOrEmpty(Path) && VhdPathResolver.IsVhdFilePath(Path))
		{
			Path = PathUtility.GetFullPath(Path, base.CurrentFileSystemLocation);
		}
		if (SupportPersistentReservations.IsPresent && !ControllerType.HasValue)
		{
			ControllerType = global::Microsoft.HyperV.PowerShell.ControllerType.SCSI;
		}
		base.NormalizeParameters();
	}

	protected override void ValidateParameters()
	{
		bool num = ControllerType == global::Microsoft.HyperV.PowerShell.ControllerType.PMEM;
		if ((!string.IsNullOrEmpty(ResourcePoolName) || MaximumIOPS.HasValue || MinimumIOPS.HasValue || IsParameterSpecified("SupportPersistentReservations")) && string.IsNullOrEmpty(Path))
		{
			throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.VMHardDiskDrive_NoPathSpecified);
		}
		if (SupportPersistentReservations.IsPresent && ControllerType != global::Microsoft.HyperV.PowerShell.ControllerType.SCSI)
		{
			throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.VMHardDiskDrive_IncompatibleWithPersistentReservations);
		}
		if ((MaximumIOPS.HasValue || MinimumIOPS.HasValue || IsParameterSpecified("QoSPolicyID") || IsParameterSpecified("QoSPolicy")) && IsParameterSpecified("DiskNumber"))
		{
			throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.VMHardDiskDrive_StorageQos_PassthroughDisksNotSupported);
		}
		if (num)
		{
			if (MaximumIOPS.HasValue || MinimumIOPS.HasValue || IsParameterSpecified("QoSPolicyID") || IsParameterSpecified("QoSPolicy"))
			{
				throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.VMHardDiskDrive_StorageQos_VirtualPMEMNotSupported);
			}
			if (IsParameterSpecified("SupportPersistentReservations"))
			{
				throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.VMHardDiskDrive_SupportsPersistentReservations_VirtualPMEMNotSupported);
			}
			if (IsParameterSpecified("DiskNumber"))
			{
				throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.VMHardDiskDrive_VirutalPMEM_PassthroughDisksNotSupported);
			}
			if (IsParameterSpecified("OverrideCacheAttributes"))
			{
				throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.VMHardDiskDrive_VirutalPMEM_CacheOverrideNotSupported);
			}
		}
		if (IsParameterSpecified("QoSPolicyID") && IsParameterSpecified("QoSPolicy"))
		{
			throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.VMHardDiskDrive_StorageQoS_MultiplePolicyParameters);
		}
		if (IsParameterSpecified("QoSPolicyID"))
		{
			Guid result;
			if (string.IsNullOrEmpty(QoSPolicyID))
			{
				Guid empty = Guid.Empty;
				QoSPolicyID = empty.ToString();
			}
			else if (!Guid.TryParse(QoSPolicyID, out result))
			{
				throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.VMHardDiskDrive_StorageQos_InvalidPolicyID);
			}
		}
		else if (IsParameterSpecified("QoSPolicy"))
		{
			if (QoSPolicy == null)
			{
				Guid empty = Guid.Empty;
				QoSPolicyID = empty.ToString();
			}
			else
			{
				if (!QoSPolicy.CimClass.CimSystemProperties.ClassName.Equals("MSFT_StorageQoSPolicy", StringComparison.OrdinalIgnoreCase))
				{
					throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.VMHardDiskDrive_StorageQoS_InvalidPolicyInstance);
				}
				if (QoSPolicy.CimInstanceProperties["PolicyId"].Value == null)
				{
					throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.VMHardDiskDrive_StorageQoS_InvalidPolicyInstance);
				}
				QoSPolicyID = QoSPolicy.CimInstanceProperties["PolicyId"].Value.ToString();
			}
		}
		else
		{
			QoSPolicyID = null;
		}
		base.ValidateParameters();
	}

	internal override IList<Tuple<VMDriveController, int>> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		IEnumerable<Tuple<VMDriveController, int>> source;
		if (CurrentParameterSetIs("VMDriveController"))
		{
			VMDriveController vMDriveController = VMDriveController;
			int item = vMDriveController.FindFirstVacantLocation();
			source = new List<Tuple<VMDriveController, int>> { Tuple.Create(vMDriveController, item) };
		}
		else
		{
			source = ParameterResolvers.ResolveVirtualMachines(this, operationWatcher).SelectWithLogging((VirtualMachine vm) => VMDriveController.FindControllerVacancy(vm, ControllerType, ControllerNumber, ControllerLocation), operationWatcher);
		}
		return source.ToList();
	}

	internal override void ProcessOneOperand(Tuple<VMDriveController, int> controllerWithLocation, IOperationWatcher operationWatcher)
	{
		VMDriveController item = controllerWithLocation.Item1;
		int item2 = controllerWithLocation.Item2;
		VirtualMachine parentAs = item.GetParentAs<VirtualMachine>();
		if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_AddVMHardDiskDrive, parentAs.Name)))
		{
			bool flag = false;
			if (!string.IsNullOrEmpty(Path) && parentAs.IsClustered)
			{
				ClusterUtilities.EnsureClusterPathValid(parentAs, Path, AllowUnverifiedPaths.IsPresent);
				flag = true;
			}
			HardDiskDrive output = (HardDiskDrive)GetRequestedConfiguration(parentAs, item, item2).AddDrive(operationWatcher);
			if (flag)
			{
				ClusterUtilities.UpdateClusterVMConfiguration(parentAs, base.InvokeCommand, operationWatcher);
			}
			if (Passthru.IsPresent)
			{
				operationWatcher.WriteObject(output);
			}
		}
	}

	private HardDriveConfigurationData GetRequestedConfiguration(VirtualMachine vm, VMDriveController controller, int controllerLocation)
	{
		HardDriveConfigurationData hardDriveConfigurationData = new HardDriveConfigurationData(vm);
		hardDriveConfigurationData.Controller = controller;
		hardDriveConfigurationData.ControllerLocation = controllerLocation;
		switch (hardDriveConfigurationData.AttachedDiskType = ((!IsParameterSpecified("DiskNumber")) ? ((!string.IsNullOrEmpty(Path)) ? AttachedDiskType.Virtual : AttachedDiskType.None) : AttachedDiskType.Physical))
		{
		case AttachedDiskType.Physical:
			hardDriveConfigurationData.SetRequestedPhysicalDrive(DiskNumber);
			break;
		case AttachedDiskType.Virtual:
			if (Path != null)
			{
				hardDriveConfigurationData.VirtualDiskPath = Path;
			}
			if (ResourcePoolName != null)
			{
				hardDriveConfigurationData.ResourcePoolName = ResourcePoolName;
			}
			if (MaximumIOPS.HasValue)
			{
				hardDriveConfigurationData.MaximumIOPS = MaximumIOPS.Value;
			}
			if (MinimumIOPS.HasValue)
			{
				hardDriveConfigurationData.MinimumIOPS = MinimumIOPS.Value;
			}
			if (QoSPolicyID != null)
			{
				hardDriveConfigurationData.QoSPolicyID = new Guid(QoSPolicyID);
			}
			if (IsParameterSpecified("SupportPersistentReservations"))
			{
				hardDriveConfigurationData.SupportPersistentReservations = SupportPersistentReservations.IsPresent;
			}
			if (OverrideCacheAttributes.HasValue)
			{
				hardDriveConfigurationData.WriteHardeningMethod = OverrideCacheAttributes.Value;
			}
			break;
		}
		return hardDriveConfigurationData;
	}
}

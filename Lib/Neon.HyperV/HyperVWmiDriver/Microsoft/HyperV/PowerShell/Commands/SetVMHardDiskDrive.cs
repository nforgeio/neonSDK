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

[Cmdlet("Set", "VMHardDiskDrive", SupportsShouldProcess = true, DefaultParameterSetName = "VMName")]
[OutputType(new Type[] { typeof(HardDiskDrive) })]
internal sealed class SetVMHardDiskDrive : VirtualizationCmdlet<HardDiskDrive>, ISupportsPassthrough, IVmBySingularVMNameCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet
{
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "VMName", ValueFromPipelineByPropertyName = true)]
    [Parameter(ParameterSetName = "Object", ValueFromPipelineByPropertyName = true)]
    [ValidateNotNullOrEmpty]
    public override CimSession[] CimSession { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "VMName")]
    [Parameter(ParameterSetName = "Object")]
    [Alias(new string[] { "PSComputerName" })]
    [ValidateNotNullOrEmpty]
    public override string[] ComputerName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "VMName")]
    [Parameter(ParameterSetName = "Object")]
    [ValidateNotNullOrEmpty]
    [CredentialArray]
    public override PSCredential[] Credential { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "Object")]
    public HardDiskDrive[] VMHardDiskDrive { get; set; }

    [ValidateNotNullOrEmpty]
    [Parameter(ParameterSetName = "VMName", Position = 0, Mandatory = true)]
    public string VMName { get; set; }

    [Parameter(ParameterSetName = "VMName", Position = 1)]
    public ControllerType ControllerType { get; set; }

    [ValidateNotNull]
    [Parameter(ParameterSetName = "VMName", Position = 2)]
    public int? ControllerNumber { get; set; }

    [ValidateNotNull]
    [Parameter(ParameterSetName = "VMName", Position = 3)]
    public int? ControllerLocation { get; set; }

    [ValidateNotNullOrEmpty]
    [Parameter(Position = 4)]
    public string Path { get; set; }

    [ValidateNotNull]
    [Parameter]
    public ControllerType? ToControllerType { get; set; }

    [ValidateNotNull]
    [Parameter]
    public int? ToControllerNumber { get; set; }

    [ValidateNotNull]
    [Parameter]
    public int? ToControllerLocation { get; set; }

    [Alias(new string[] { "Number" })]
    [Parameter(ValueFromPipelineByPropertyName = true)]
    public uint DiskNumber { get; set; }

    [ValidateNotNull]
    [Parameter]
    public string ResourcePoolName { get; set; }

    [ValidateNotNullOrEmpty]
    [Parameter]
    [Alias(new string[] { "ShareVirtualDisk" })]
    public bool? SupportPersistentReservations { get; set; }

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

    private bool HasVirtualDiskParameters
    {
        get
        {
            if (string.IsNullOrEmpty(Path) && !IsParameterSpecified("ResourcePoolName") && !MaximumIOPS.HasValue && !MinimumIOPS.HasValue && !IsParameterSpecified("QoSPolicyID") && !IsParameterSpecified("QoSPolicy"))
            {
                return SupportPersistentReservations.HasValue;
            }
            return true;
        }
    }

    private bool HasPhysicalDiskParameters => IsParameterSpecified("DiskNumber");

    protected override void NormalizeParameters()
    {
        base.NormalizeParameters();
        if (!string.IsNullOrEmpty(Path) && VhdPathResolver.IsVhdFilePath(Path))
        {
            Path = PathUtility.GetFullPath(Path, base.CurrentFileSystemLocation);
        }
    }

    protected override void ValidateParameters()
    {
        base.ValidateParameters();
        if (HasVirtualDiskParameters && HasPhysicalDiskParameters)
        {
            throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.InvalidParameter_PhysicalAndVirtualDiskSettingsBothProvided);
        }
        if (IsParameterSpecified("Path") && string.IsNullOrEmpty(Path))
        {
            throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.InvalidParameter_PathCannotBeNull);
        }
        bool flag = (IsParameterSpecified("ControllerType") && ControllerType != ControllerType.SCSI && !ToControllerType.HasValue) || (ToControllerType.HasValue && ToControllerType.Value != ControllerType.SCSI);
        if (SupportPersistentReservations.Equals(true) && flag)
        {
            throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.VMHardDiskDrive_IncompatibleWithPersistentReservations);
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
                return;
            }
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
        else
        {
            QoSPolicyID = null;
        }
    }

    internal override IList<HardDiskDrive> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        if (CurrentParameterSetIs("Object"))
        {
            return VMHardDiskDrive;
        }
        return ParameterResolvers.ResolveVirtualMachines(this, operationWatcher).SelectManyWithLogging((VirtualMachine vm) => vm.FindDrives(ControllerType, ControllerNumber, ControllerLocation), operationWatcher).OfType<HardDiskDrive>()
            .ToList();
    }

    internal override void ProcessOneOperand(HardDiskDrive hardDrive, IOperationWatcher operationWatcher)
    {
        VirtualMachineBase parentAs = hardDrive.GetParentAs<VirtualMachineBase>();
        if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_SetVMHardDiskDrive, hardDrive, parentAs.Name)))
        {
            bool flag = false;
            if (!string.IsNullOrEmpty(Path) && parentAs.IsClustered)
            {
                ClusterUtilities.EnsureClusterPathValid(parentAs.GetVirtualMachine(), Path, AllowUnverifiedPaths.IsPresent);
                flag = true;
            }
            HardDriveConfigurationData originalConfiguration = (HardDriveConfigurationData)hardDrive.GetCurrentConfiguration();
            HardDriveConfigurationData requestedConfiguration = GetRequestedConfiguration(hardDrive, parentAs, originalConfiguration);
            hardDrive.Configure(requestedConfiguration, originalConfiguration, operationWatcher);
            if (flag)
            {
                ClusterUtilities.UpdateClusterVMConfiguration(parentAs, base.InvokeCommand, operationWatcher);
            }
            if (Passthru.IsPresent)
            {
                operationWatcher.WriteObject(hardDrive);
            }
        }
    }

    private void LoadPhysicalDiskConfigurationValues(HardDriveConfigurationData newConfiguration, HardDriveConfigurationData oldConfiguration)
    {
        if (IsParameterSpecified("DiskNumber"))
        {
            newConfiguration.SetRequestedPhysicalDrive(DiskNumber);
        }
        else
        {
            newConfiguration.CopyRequestedPhysicalDrive(oldConfiguration);
        }
    }

    private void LoadVirtualDiskConfigurationValues(HardDriveConfigurationData newConfiguration, HardDriveConfigurationData oldConfiguration)
    {
        newConfiguration.VirtualDiskPath = Path ?? oldConfiguration.VirtualDiskPath;
        newConfiguration.ResourcePoolName = ResourcePoolName ?? oldConfiguration.ResourcePoolName;
        newConfiguration.MaximumIOPS = MaximumIOPS ?? oldConfiguration.MaximumIOPS;
        newConfiguration.MinimumIOPS = MinimumIOPS ?? oldConfiguration.MinimumIOPS;
        newConfiguration.QoSPolicyID = ((QoSPolicyID != null) ? new Guid?(new Guid(QoSPolicyID)) : oldConfiguration.QoSPolicyID);
        newConfiguration.SupportPersistentReservations = SupportPersistentReservations ?? oldConfiguration.SupportPersistentReservations;
    }

    private HardDriveConfigurationData GetRequestedConfiguration(HardDiskDrive hardDrive, VirtualMachineBase parent, HardDriveConfigurationData originalConfiguration)
    {
        HardDriveConfigurationData hardDriveConfigurationData = new HardDriveConfigurationData(parent);
        Tuple<VMDriveController, int> tuple = VMDriveController.FindControllerVacancyForMove(hardDrive, ToControllerType, ToControllerNumber, ToControllerLocation);
        if (tuple != null)
        {
            hardDriveConfigurationData.Controller = tuple.Item1;
            hardDriveConfigurationData.ControllerLocation = tuple.Item2;
        }
        else
        {
            hardDriveConfigurationData.Controller = originalConfiguration.Controller;
            hardDriveConfigurationData.ControllerLocation = originalConfiguration.ControllerLocation;
        }
        switch (hardDriveConfigurationData.AttachedDiskType = (HasPhysicalDiskParameters ? AttachedDiskType.Physical : (HasVirtualDiskParameters ? AttachedDiskType.Virtual : originalConfiguration.AttachedDiskType)))
        {
        case AttachedDiskType.Physical:
            LoadPhysicalDiskConfigurationValues(hardDriveConfigurationData, originalConfiguration);
            break;
        case AttachedDiskType.Virtual:
            LoadVirtualDiskConfigurationValues(hardDriveConfigurationData, originalConfiguration);
            break;
        }
        if (OverrideCacheAttributes.HasValue)
        {
            hardDriveConfigurationData.WriteHardeningMethod = OverrideCacheAttributes.Value;
        }
        if (hardDriveConfigurationData.Controller.GetType() == typeof(VMPmemController))
        {
            if (MaximumIOPS.HasValue || MinimumIOPS.HasValue || IsParameterSpecified("QoSPolicyID") || IsParameterSpecified("QoSPolicy"))
            {
                throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.VMHardDiskDrive_StorageQos_VirtualPMEMNotSupported);
            }
            if (IsParameterSpecified("SupportPersistentReservations"))
            {
                throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.VMHardDiskDrive_SupportsPersistentReservations_VirtualPMEMNotSupported);
            }
            if (HasPhysicalDiskParameters)
            {
                throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.VMHardDiskDrive_VirutalPMEM_PassthroughDisksNotSupported);
            }
            if (IsParameterSpecified("OverrideCacheAttributes"))
            {
                throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.VMHardDiskDrive_VirutalPMEM_CacheOverrideNotSupported);
            }
        }
        return hardDriveConfigurationData;
    }
}

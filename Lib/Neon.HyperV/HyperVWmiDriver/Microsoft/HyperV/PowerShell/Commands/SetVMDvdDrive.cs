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

[Cmdlet("Set", "VMDvdDrive", SupportsShouldProcess = true, DefaultParameterSetName = "VMName")]
[OutputType(new Type[] { typeof(DvdDrive) })]
internal sealed class SetVMDvdDrive : VirtualizationCmdlet<DvdDrive>, ISupportsPassthrough, IVmBySingularVMNameCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet
{
    internal static class AdditionalParameterSetNames
    {
        public const string Object = "Object";
    }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "VMName")]
    [ValidateNotNullOrEmpty]
    public override CimSession[] CimSession { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "VMName")]
    [ValidateNotNullOrEmpty]
    public override string[] ComputerName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "VMName")]
    [ValidateNotNullOrEmpty]
    [CredentialArray]
    public override PSCredential[] Credential { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "Object")]
    public DvdDrive[] VMDvdDrive { get; set; }

    [ValidateNotNullOrEmpty]
    [Parameter(ParameterSetName = "VMName", Position = 0, Mandatory = true)]
    public string VMName { get; set; }

    [ValidateNotNull]
    [Parameter(ParameterSetName = "VMName", Position = 1)]
    public int? ControllerNumber { get; set; }

    [ValidateNotNull]
    [Parameter(ParameterSetName = "VMName", Position = 2)]
    public int? ControllerLocation { get; set; }

    [ValidateNotNull]
    [Parameter]
    public int? ToControllerNumber { get; set; }

    [ValidateNotNull]
    [Parameter]
    public int? ToControllerLocation { get; set; }

    [Parameter(Position = 3)]
    public string Path { get; set; }

    [ValidateNotNullOrEmpty]
    [Parameter]
    public string ResourcePoolName { get; set; }

    [Parameter]
    public SwitchParameter AllowUnverifiedPaths { get; set; }

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
    [Parameter]
    public SwitchParameter Passthru { get; set; }

    protected override void NormalizeParameters()
    {
        if (!string.IsNullOrEmpty(Path))
        {
            Path = PathUtility.GetFullPath(Path, base.CurrentFileSystemLocation).TrimEnd('\\');
        }
        base.NormalizeParameters();
    }

    protected override void ValidateParameters()
    {
        if (!string.IsNullOrEmpty(Path) && !Utilities.IsIsoFilePath(Path) && !Utilities.IsDriveVolumeString(Path))
        {
            throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.VMDvdDrive_InvalidDvdDrivePath);
        }
        base.ValidateParameters();
    }

    internal override IList<DvdDrive> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        if (VMDvdDrive != null)
        {
            return VMDvdDrive;
        }
        return ParameterResolvers.ResolveVirtualMachines(this, operationWatcher).SelectManyWithLogging((VirtualMachine vm) => vm.FindDrives((vm.Generation == 2) ? ControllerType.SCSI : ControllerType.IDE, ControllerNumber, ControllerLocation), operationWatcher).OfType<DvdDrive>()
            .ToList();
    }

    internal override void ProcessOneOperand(DvdDrive dvdDrive, IOperationWatcher operationWatcher)
    {
        VirtualMachine parentAs = dvdDrive.GetParentAs<VirtualMachine>();
        if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_SetVMDvdDrive, dvdDrive, parentAs.Name)))
        {
            bool flag = false;
            if (!string.IsNullOrEmpty(Path) && parentAs.IsClustered)
            {
                ClusterUtilities.EnsureClusterPathValid(parentAs, Path, AllowUnverifiedPaths.IsPresent);
                flag = true;
            }
            DvdDriveConfigurationData originalConfiguration = (DvdDriveConfigurationData)dvdDrive.GetCurrentConfiguration();
            DvdDriveConfigurationData requestedConfiguration = GetRequestedConfiguration(dvdDrive, parentAs, originalConfiguration);
            dvdDrive.Configure(requestedConfiguration, originalConfiguration, operationWatcher);
            if (flag)
            {
                ClusterUtilities.UpdateClusterVMConfiguration(parentAs, base.InvokeCommand, operationWatcher);
            }
            if (Passthru.IsPresent)
            {
                operationWatcher.WriteObject(dvdDrive);
            }
        }
    }

    private DvdDriveConfigurationData GetRequestedConfiguration(DvdDrive dvdDrive, VirtualMachine parent, DvdDriveConfigurationData originalConfiguration)
    {
        DvdDriveConfigurationData dvdDriveConfigurationData = new DvdDriveConfigurationData(parent);
        Tuple<VMDriveController, int> tuple = VMDriveController.FindControllerVacancyForMove(dvdDrive, (parent.Generation == 2) ? ControllerType.SCSI : ControllerType.IDE, ToControllerNumber, ToControllerLocation);
        if (tuple != null)
        {
            dvdDriveConfigurationData.Controller = tuple.Item1;
            dvdDriveConfigurationData.ControllerLocation = tuple.Item2;
        }
        else
        {
            dvdDriveConfigurationData.Controller = originalConfiguration.Controller;
            dvdDriveConfigurationData.ControllerLocation = originalConfiguration.ControllerLocation;
        }
        switch (dvdDriveConfigurationData.AttachedDiskType = ((!IsParameterSpecified("Path")) ? originalConfiguration.AttachedDiskType : ((!string.IsNullOrEmpty(Path)) ? ((!Utilities.IsDriveVolumeString(Path)) ? AttachedDiskType.Virtual : AttachedDiskType.Physical) : AttachedDiskType.None)))
        {
        case AttachedDiskType.Physical:
            if (!string.IsNullOrEmpty(Path))
            {
                dvdDriveConfigurationData.SetRequestedPhysicalDrive(Path);
            }
            else
            {
                dvdDriveConfigurationData.CopyRequestedPhysicalDrive(originalConfiguration);
            }
            break;
        case AttachedDiskType.Virtual:
            dvdDriveConfigurationData.VirtualDiskPath = Path ?? originalConfiguration.VirtualDiskPath;
            dvdDriveConfigurationData.ResourcePoolName = ResourcePoolName ?? originalConfiguration.ResourcePoolName;
            break;
        }
        return dvdDriveConfigurationData;
    }
}

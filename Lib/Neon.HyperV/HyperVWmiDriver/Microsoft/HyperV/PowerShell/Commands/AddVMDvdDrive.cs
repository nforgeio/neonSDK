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

[Cmdlet("Add", "VMDvdDrive", SupportsShouldProcess = true, DefaultParameterSetName = "VMName")]
[OutputType(new Type[] { typeof(DvdDrive) })]
internal sealed class AddVMDvdDrive : VirtualizationCmdlet<Tuple<VMDriveController, int>>, IVMObjectOrVMNameCmdlet, IVmByObjectCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmByVMNameCmdlet, ISupportsPassthrough
{
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
    [ValidateNotNullOrEmpty]
    [Parameter(ParameterSetName = "VMName", ValueFromPipeline = true, Position = 0, Mandatory = true)]
    public string[] VMName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter(ParameterSetName = "VMObject", ValueFromPipeline = true, Position = 0, Mandatory = true)]
    public VirtualMachine[] VM { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "VMDriveController")]
    public VMDriveController[] VMDriveController { get; set; }

    [Parameter(Position = 2, ParameterSetName = "VMObject")]
    [Parameter(Position = 2, ParameterSetName = "VMName")]
    [ValidateNotNull]
    public int? ControllerNumber { get; set; }

    [Parameter(Position = 3)]
    [ValidateNotNull]
    public int? ControllerLocation { get; set; }

    [Parameter(Position = 4)]
    public string Path { get; set; }

    [ValidateNotNull]
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

    internal override IList<Tuple<VMDriveController, int>> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        IEnumerable<Tuple<VMDriveController, int>> source = ((!CurrentParameterSetIs("VMDriveController")) ? ParameterResolvers.ResolveVirtualMachines(this, operationWatcher).SelectWithLogging((VirtualMachine vm) => global::Microsoft.HyperV.PowerShell.VMDriveController.FindControllerVacancy(vm, (vm.Generation == 2) ? ControllerType.SCSI : ControllerType.IDE, ControllerNumber, ControllerLocation), operationWatcher) : VMDriveController.SelectWithLogging((VMDriveController controller) => Tuple.Create(controller, controller.FindFirstVacantLocation()), operationWatcher));
        return source.ToList();
    }

    internal override void ProcessOneOperand(Tuple<VMDriveController, int> controllerWithLocation, IOperationWatcher operationWatcher)
    {
        VMDriveController item = controllerWithLocation.Item1;
        int item2 = controllerWithLocation.Item2;
        VirtualMachine parentAs = item.GetParentAs<VirtualMachine>();
        if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_AddVMDvdDrive, parentAs.Name)))
        {
            bool flag = false;
            if (!string.IsNullOrEmpty(Path) && parentAs.IsClustered)
            {
                ClusterUtilities.EnsureClusterPathValid(parentAs, Path, AllowUnverifiedPaths.IsPresent);
                flag = true;
            }
            DvdDrive output = (DvdDrive)GetRequestedConfiguration(parentAs, item, item2).AddDrive(operationWatcher);
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

    private DvdDriveConfigurationData GetRequestedConfiguration(VirtualMachine vm, VMDriveController controller, int controllerLocation)
    {
        DvdDriveConfigurationData dvdDriveConfigurationData = new DvdDriveConfigurationData(vm);
        if (string.IsNullOrEmpty(Path))
        {
            dvdDriveConfigurationData.AttachedDiskType = AttachedDiskType.None;
        }
        else
        {
            dvdDriveConfigurationData.AttachedDiskType = (Utilities.IsIsoFilePath(Path) ? AttachedDiskType.Virtual : AttachedDiskType.Physical);
        }
        dvdDriveConfigurationData.Controller = controller;
        dvdDriveConfigurationData.ControllerLocation = controllerLocation;
        if (Path != null)
        {
            if (dvdDriveConfigurationData.AttachedDiskType == AttachedDiskType.Virtual)
            {
                dvdDriveConfigurationData.VirtualDiskPath = Path;
            }
            else
            {
                dvdDriveConfigurationData.SetRequestedPhysicalDrive(Path);
            }
        }
        if (ResourcePoolName != null)
        {
            dvdDriveConfigurationData.ResourcePoolName = ResourcePoolName;
        }
        return dvdDriveConfigurationData;
    }
}

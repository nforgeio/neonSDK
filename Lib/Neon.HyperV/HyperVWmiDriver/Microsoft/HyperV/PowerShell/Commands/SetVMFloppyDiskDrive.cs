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

[Cmdlet("Set", "VMFloppyDiskDrive", DefaultParameterSetName = "VMName", SupportsShouldProcess = true)]
[OutputType(new Type[] { typeof(VMFloppyDiskDrive) })]
internal sealed class SetVMFloppyDiskDrive : VirtualizationCmdlet<VMFloppyDiskDrive>, IVMObjectOrVMNameCmdlet, IVmByObjectCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmByVMNameCmdlet, ISupportsPassthrough
{
    private static class ParameterSetNames
    {
        public const string VMFloppyDiskDrive = "VMFloppyDiskDrive";
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
    [ValidateNotNullOrEmpty]
    [Parameter(ParameterSetName = "VMName", ValueFromPipeline = true, Position = 0, Mandatory = true)]
    public string[] VMName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter(ParameterSetName = "VMObject", ValueFromPipeline = true, Position = 0, Mandatory = true)]
    public VirtualMachine[] VM { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "VMFloppyDiskDrive")]
    public VMFloppyDiskDrive[] VMFloppyDiskDrive { get; set; }

    [Alias(new string[] { "FullName" })]
    [Parameter(Position = 1, ValueFromPipelineByPropertyName = true)]
    public string Path { get; set; }

    [ValidateNotNullOrEmpty]
    [Parameter]
    public string ResourcePoolName { get; set; }

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
    [Parameter]
    public SwitchParameter Passthru { get; set; }

    protected override void ValidateParameters()
    {
        if (string.IsNullOrEmpty(Path) && !string.IsNullOrEmpty(ResourcePoolName))
        {
            throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.VMDrive_NoPathSpecifiedWithPool);
        }
        if (Path != null && !Utilities.IsVfdFilePath(Path))
        {
            throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.SetVMFloppyDiskDrive_InvalidPath);
        }
        base.ValidateParameters();
    }

    internal override IList<VMFloppyDiskDrive> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        if (CurrentParameterSetIs("VMFloppyDiskDrive"))
        {
            return VMFloppyDiskDrive;
        }
        return ParameterResolvers.ResolveVirtualMachines(this, operationWatcher).SelectWithLogging((VirtualMachine vm) => vm.FloppyDrive, operationWatcher).ToList();
    }

    internal override void ProcessOneOperand(VMFloppyDiskDrive floppyDrive, IOperationWatcher operationWatcher)
    {
        if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_SetVM, floppyDrive.VMName)))
        {
            FloppyDriveConfigurationData floppyDriveConfigurationData = (FloppyDriveConfigurationData)floppyDrive.GetCurrentConfiguration();
            FloppyDriveConfigurationData floppyDriveConfigurationData2 = new FloppyDriveConfigurationData(floppyDrive.GetParentAs<VirtualMachineBase>());
            if (IsParameterSpecified("Path"))
            {
                floppyDriveConfigurationData2.AttachedDiskType = ((Path != null) ? AttachedDiskType.Virtual : AttachedDiskType.None);
            }
            else
            {
                floppyDriveConfigurationData2.AttachedDiskType = floppyDriveConfigurationData.AttachedDiskType;
            }
            if (floppyDriveConfigurationData2.AttachedDiskType == AttachedDiskType.Virtual)
            {
                floppyDriveConfigurationData2.VirtualDiskPath = Path ?? floppyDriveConfigurationData.VirtualDiskPath;
                floppyDriveConfigurationData2.ResourcePoolName = ResourcePoolName ?? floppyDriveConfigurationData.ResourcePoolName;
            }
            floppyDrive.Configure(floppyDriveConfigurationData2, floppyDriveConfigurationData, operationWatcher);
            if (Passthru.IsPresent)
            {
                operationWatcher.WriteObject(floppyDrive);
            }
        }
    }
}

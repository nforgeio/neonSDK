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

[Cmdlet("Add", "VMKeyStorageDrive", SupportsShouldProcess = true, DefaultParameterSetName = "VMName")]
[OutputType(new Type[] { typeof(KeyStorageDrive) })]
internal sealed class AddVMKeyStorageDrive : VirtualizationCmdlet<Tuple<VMDriveController, int>>, IVMObjectOrVMNameCmdlet, IVmByObjectCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmByVMNameCmdlet, ISupportsPassthrough
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

    [ValidateNotNull]
    [Parameter(Position = 2, ParameterSetName = "VMObject")]
    [Parameter(Position = 2, ParameterSetName = "VMName")]
    public int? ControllerNumber { get; set; }

    [ValidateNotNull]
    [Parameter(Position = 3)]
    public int? ControllerLocation { get; set; }

    [ValidateNotNull]
    [Parameter]
    public string ResourcePoolName { get; set; }

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
    [Parameter]
    public SwitchParameter Passthru { get; set; }

    internal override IList<Tuple<VMDriveController, int>> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        operationWatcher.WriteWarning(string.Format(CultureInfo.CurrentCulture, CmdletResources.KeyStorageDriveDeprecatedWarning));
        return ParameterResolvers.ResolveVirtualMachines(this, operationWatcher).SelectWithLogging((VirtualMachine vm) => VMDriveController.FindControllerVacancy(vm, ControllerType.IDE, ControllerNumber, ControllerLocation), operationWatcher).ToList();
    }

    internal override void ProcessOneOperand(Tuple<VMDriveController, int> controllerWithLocation, IOperationWatcher operationWatcher)
    {
        VMDriveController item = controllerWithLocation.Item1;
        int item2 = controllerWithLocation.Item2;
        VirtualMachine parentAs = item.GetParentAs<VirtualMachine>();
        if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_AddVMKeyStorageDrive, parentAs.Name)))
        {
            if (parentAs.FindDrives(ControllerType.IDE, null, null).OfType<KeyStorageDrive>().Count() != 0)
            {
                throw ExceptionHelper.CreateOperationFailedException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.NewVMKeyStorageDrive_CannotAddMultiple, parentAs.Name));
            }
            VMSecurity security = parentAs.GetSecurity();
            security.KsdEnabled = true;
            ((IUpdatable)security).Put(operationWatcher);
            KeyStorageDrive output = (KeyStorageDrive)GetRequestedConfiguration(parentAs, item, item2).AddDrive(operationWatcher);
            if (parentAs.IsClustered)
            {
                ClusterUtilities.UpdateClusterVMConfiguration(parentAs, base.InvokeCommand, operationWatcher);
            }
            if (Passthru.IsPresent)
            {
                operationWatcher.WriteObject(output);
            }
        }
    }

    private KeyStorageDriveConfigurationData GetRequestedConfiguration(VirtualMachine vm, VMDriveController controller, int controllerLocation)
    {
        KeyStorageDriveConfigurationData keyStorageDriveConfigurationData = new KeyStorageDriveConfigurationData(vm);
        keyStorageDriveConfigurationData.AttachedDiskType = AttachedDiskType.None;
        keyStorageDriveConfigurationData.Controller = controller;
        keyStorageDriveConfigurationData.ControllerLocation = controllerLocation;
        if (ResourcePoolName != null)
        {
            keyStorageDriveConfigurationData.ResourcePoolName = ResourcePoolName;
        }
        return keyStorageDriveConfigurationData;
    }
}

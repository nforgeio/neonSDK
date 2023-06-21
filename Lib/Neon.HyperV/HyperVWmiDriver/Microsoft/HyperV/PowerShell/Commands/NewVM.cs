using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.Vhd.PowerShell;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("New", "VM", SupportsShouldProcess = true, DefaultParameterSetName = "No VHD")]
[OutputType(new Type[] { typeof(VirtualMachine) })]
internal sealed class NewVM : VirtualizationCreationCmdlet<VirtualMachine>, ISupportsAsJob, ISupportsForce
{
    internal const string gm_VirtualHardDriveDirectory = "Virtual Hard Disks";

    [ValidateNotNullOrEmpty]
    [Parameter(Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
    [Alias(new string[] { "VMName" })]
    public string Name { get; set; }

    [ValidateNotNull]
    [Parameter(Position = 1)]
    public long? MemoryStartupBytes { get; set; }

    [Parameter]
    [ValidateNotNullOrEmpty]
    public BootDevice? BootDevice { get; set; }

    [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "VHD", Justification = "This is per spec.")]
    [Parameter(ParameterSetName = "No VHD")]
    public SwitchParameter NoVHD { get; set; }

    [ValidateNotNullOrEmpty]
    [Parameter]
    public string SwitchName { get; set; }

    [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "VHD", Justification = "This is per spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter(ParameterSetName = "New VHD", Mandatory = true)]
    public string NewVHDPath { get; set; }

    [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "VHD", Justification = "This is per spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter(ParameterSetName = "New VHD", Mandatory = true)]
    public ulong NewVHDSizeBytes { get; set; }

    [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "VHD", Justification = "This is per spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter(Mandatory = true, ParameterSetName = "Existing VHD")]
    public string VHDPath { get; set; }

    [ValidateNotNullOrEmpty]
    [Parameter]
    public string Path { get; set; }

    [ValidateNotNull]
    [Parameter]
    public Version Version { get; set; }

    [Parameter]
    public SwitchParameter Prerelease { get; set; }

    [Parameter(DontShow = true)]
    public SwitchParameter Experimental { get; set; }

    [ValidateNotNullOrEmpty]
    [ValidateSet(new string[] { "1", "2" })]
    [Parameter(Position = 2)]
    public short? Generation { get; set; }

    [Parameter]
    public SwitchParameter Force { get; set; }

    [Parameter]
    public SwitchParameter AsJob { get; set; }

    protected override void NormalizeParameters()
    {
        base.NormalizeParameters();
        if (string.IsNullOrEmpty(Name))
        {
            Name = CmdletResources.DefaultVMName;
        }
        if (!string.IsNullOrEmpty(Path))
        {
            Path = PathUtility.GetFullPath(Path, base.CurrentFileSystemLocation);
            Path = global::System.IO.Path.Combine(Path, Name);
        }
        if (!Generation.HasValue)
        {
            Generation = 1;
        }
        if ((bool)Experimental)
        {
            ValidateMutuallyExclusiveParameters("Version", "Experimental");
            Version = new Version(255, 0);
        }
        if ((bool)Prerelease)
        {
            ValidateMutuallyExclusiveParameters("Version", "Prerelease");
            Version = new Version(254, 0);
        }
    }

    protected override void ValidateParameters()
    {
        base.ValidateParameters();
        if (!string.IsNullOrEmpty(NewVHDPath))
        {
            string extension = global::System.IO.Path.GetExtension(NewVHDPath);
            if (string.IsNullOrEmpty(extension) || !string.Equals(extension, ".VHDX", StringComparison.OrdinalIgnoreCase))
            {
                throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.InvalidParameter_RequiresVhdxExtension);
            }
        }
        if (Generation == 2 && BootDevice.HasValue)
        {
            BootDevice value = BootDevice.Value;
            string text = null;
            switch (value)
            {
            case global::Microsoft.HyperV.PowerShell.BootDevice.Floppy:
                text = CmdletErrorMessages.Generation2_NoFloppySupport;
                break;
            case global::Microsoft.HyperV.PowerShell.BootDevice.IDE:
                text = CmdletErrorMessages.Generation2_NoIDESupport;
                break;
            case global::Microsoft.HyperV.PowerShell.BootDevice.LegacyNetworkAdapter:
                text = CmdletErrorMessages.Generation2_NoLegacyNetworkAdapterSupport;
                break;
            }
            if (text != null)
            {
                throw ExceptionHelper.CreateInvalidArgumentException(text);
            }
        }
    }

    internal override IList<VirtualMachine> CreateObjects(IOperationWatcher operationWatcher)
    {
        List<VirtualMachine> list = new List<VirtualMachine>();
        foreach (Server server in ParameterResolvers.GetServers(this, operationWatcher))
        {
            VirtualMachine virtualMachine = null;
            string text = null;
            if (!operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_NewVM, Name)) || (IsPrereleaseVM() && !operationWatcher.ShouldContinue(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldContinue_NewPrereleaseVM, Name))))
            {
                continue;
            }
            try
            {
                virtualMachine = CreateVirtualMachine(server, operationWatcher);
                ConfigureVirtualMachine(virtualMachine, operationWatcher);
                ConfigureVMNetwork(virtualMachine, operationWatcher);
                text = ConfigureHardDrive(virtualMachine, operationWatcher);
                ConfigureBootDevice(virtualMachine, operationWatcher);
                list.Add(virtualMachine);
                virtualMachine = null;
                text = null;
            }
            catch (Exception e3)
            {
                if (text != null)
                {
                    try
                    {
                        Utilities.DeleteDataFiles(server, text);
                    }
                    catch (Exception innerException)
                    {
                        Exception e = ExceptionHelper.CreateInvalidStateException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.NewVM_FailedToRemoveNewVhd, text), innerException, virtualMachine);
                        ExceptionHelper.DisplayErrorOnException(e, operationWatcher);
                    }
                }
                if (virtualMachine != null)
                {
                    try
                    {
                        ((IRemovable)virtualMachine).Remove(operationWatcher);
                    }
                    catch (Exception innerException2)
                    {
                        Exception e2 = ExceptionHelper.CreateInvalidStateException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.NewVM_FailedToRollBack, virtualMachine.Name), innerException2, virtualMachine);
                        ExceptionHelper.DisplayErrorOnException(e2, operationWatcher);
                    }
                }
                ExceptionHelper.DisplayErrorOnException(e3, operationWatcher);
            }
        }
        return list;
    }

    private VirtualMachine CreateVirtualMachine(Server server, IOperationWatcher operationWatcher)
    {
        return VirtualMachine.Create(server, Name, Path, Generation.Value, Version, operationWatcher);
    }

    private void ConfigureVirtualMachine(VirtualMachine vm, IOperationWatcher operationWatcher)
    {
        if (MemoryStartupBytes.HasValue)
        {
            VMMemory memory = vm.GetMemory();
            memory.Startup = MemoryStartupBytes.Value;
            ((IUpdatable)memory).Put(operationWatcher);
        }
        VMScsiController templateScsiController = VMScsiController.CreateTemplateScsiController(vm);
        VMScsiController controller = vm.AddScsiController(templateScsiController, operationWatcher);
        if (Generation == 1)
        {
            VMDriveController vMDriveController = vm.GetIdeControllers().FirstOrDefault((VMIdeController c) => c.ControllerNumber == 1);
            if (vMDriveController == null)
            {
                throw ExceptionHelper.CreateObjectNotFoundException(CmdletErrorMessages.NewVM_FailedToGetController, null);
            }
            AddDvdDrive(vm, vMDriveController, 0, operationWatcher);
        }
        else if (BootDevice == global::Microsoft.HyperV.PowerShell.BootDevice.CD)
        {
            AddDvdDrive(vm, controller, 0, operationWatcher);
        }
    }

    private static void AddDvdDrive(VirtualMachine vm, VMDriveController controller, int controllerLocation, IOperationWatcher operationWatcher)
    {
        DvdDriveConfigurationData dvdDriveConfigurationData = new DvdDriveConfigurationData(vm);
        dvdDriveConfigurationData.Controller = controller;
        dvdDriveConfigurationData.ControllerLocation = controllerLocation;
        dvdDriveConfigurationData.AttachedDiskType = AttachedDiskType.None;
        dvdDriveConfigurationData.AddDrive(operationWatcher);
    }

    private string ConfigureHardDrive(VirtualMachine vm, IOperationWatcher operationWatcher)
    {
        string result = null;
        if (CurrentParameterSetIs("Existing VHD"))
        {
            string path = ((!string.IsNullOrEmpty(global::System.IO.Path.GetDirectoryName(VHDPath))) ? PathUtility.GetFullPath(VHDPath, base.CurrentFileSystemLocation) : global::System.IO.Path.Combine(VirtualizationObjectLocator.GetVMHost(vm.Server).VirtualHardDiskPath, VHDPath));
            AddVhdToVm(vm, path, operationWatcher);
        }
        else if (CurrentParameterSetIs("New VHD"))
        {
            string text = ((!string.IsNullOrEmpty(global::System.IO.Path.GetDirectoryName(NewVHDPath))) ? PathUtility.GetFullPath(NewVHDPath, base.CurrentFileSystemLocation) : (string.IsNullOrEmpty(Path) ? global::System.IO.Path.Combine(VirtualizationObjectLocator.GetVMHost(vm.Server).VirtualHardDiskPath, NewVHDPath) : global::System.IO.Path.Combine(global::System.IO.Path.Combine(Path, "Virtual Hard Disks"), NewVHDPath)));
            VhdUtilities.CreateVirtualHardDisk(vm.Server, VhdType.Dynamic, VhdFormat.VHDX, text, null, (long)NewVHDSizeBytes, 0L, 0L, 0L, pmemCompatible: false, VirtualHardDiskPmemAddressAbstractionType.None, 0L, operationWatcher);
            AddVhdToVm(vm, text, operationWatcher);
            result = text;
        }
        return result;
    }

    private void AddVhdToVm(VirtualMachine vm, string path, IOperationWatcher operationWatcher)
    {
        VMDriveController vMDriveController = null;
        if (Generation == 2)
        {
            if (!string.IsNullOrEmpty(path) && (path.EndsWith(".VHD", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".AVHD", StringComparison.OrdinalIgnoreCase)))
            {
                throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.Generation2_NoVhdFormatSupport);
            }
            vMDriveController = vm.GetScsiControllers().FirstOrDefault();
            if (vMDriveController == null)
            {
                throw ExceptionHelper.CreateObjectNotFoundException(CmdletErrorMessages.NewVM_FailedToGetController, null);
            }
        }
        else
        {
            vMDriveController = vm.GetIdeControllers().FirstOrDefault();
            if (vMDriveController == null)
            {
                throw ExceptionHelper.CreateObjectNotFoundException(CmdletErrorMessages.NewVM_FailedToGetController, null);
            }
        }
        HardDriveConfigurationData hardDriveConfigurationData = new HardDriveConfigurationData(vm);
        hardDriveConfigurationData.Controller = vMDriveController;
        hardDriveConfigurationData.ControllerLocation = 0;
        hardDriveConfigurationData.AttachedDiskType = AttachedDiskType.Virtual;
        hardDriveConfigurationData.VirtualDiskPath = path;
        hardDriveConfigurationData.AddDrive(operationWatcher);
    }

    private void ConfigureBootDevice(VirtualMachine vm, IOperationWatcher operationWatcher)
    {
        if (Generation == 2 && CurrentParameterSetIs("Existing VHD") && !BootDevice.HasValue)
        {
            BootDevice = global::Microsoft.HyperV.PowerShell.BootDevice.VHD;
        }
        if (!BootDevice.HasValue)
        {
            return;
        }
        if (Generation == 2)
        {
            BootDevice firstBootDevice2 = BootDevice.Value;
            VMFirmware firmware = vm.GetFirmware();
            List<VMBootSource> list = new List<VMBootSource>(firmware.BootOrder);
            int num = list.FindIndex((VMBootSource bootEntry) => bootEntry.IsBootDeviceType(firstBootDevice2));
            if (num < 0)
            {
                operationWatcher.WriteWarning(CmdletErrorMessages.NewVM_NoMatchingBootEntryFoundWarning);
            }
            else if (num > 0)
            {
                VMBootSource item = list[num];
                list.RemoveAt(num);
                list.Insert(0, item);
                firmware.BootOrder = list.ToArray();
                ((IUpdatable)firmware).Put(operationWatcher);
            }
        }
        else
        {
            BootDevice firstBootDevice = VMBios.TranslateBootDeviceValue(BootDevice.Value);
            List<BootDevice> list2 = new List<BootDevice>();
            VMBios bios = vm.GetBios();
            list2.AddRange(bios.StartupOrder);
            int num2 = list2.FindIndex((BootDevice device) => device == firstBootDevice);
            if (num2 != 0)
            {
                list2[num2] = list2[0];
                list2[0] = firstBootDevice;
            }
            bios.StartupOrder = list2.ToArray();
            ((IUpdatable)bios).Put(operationWatcher);
        }
    }

    private void ConfigureVMNetwork(VirtualMachine vm, IOperationWatcher operationWatcher)
    {
        bool isEmulated = false;
        if (Generation == 1 && BootDevice.HasValue)
        {
            switch (BootDevice.Value)
            {
            case global::Microsoft.HyperV.PowerShell.BootDevice.LegacyNetworkAdapter:
            case global::Microsoft.HyperV.PowerShell.BootDevice.NetworkAdapter:
                isEmulated = true;
                break;
            }
        }
        VMNetworkAdapter vMNetworkAdapter = VMNetworkAdapter.CreateTemplateForAdd(isEmulated, vm);
        if (!string.IsNullOrEmpty(SwitchName))
        {
            vMNetworkAdapter.SetConnectedSwitchName(SwitchName);
        }
        VMNetworkAdapter.ApplyAdd(vMNetworkAdapter, operationWatcher);
    }

    private bool IsPrereleaseVM()
    {
        if (Version != null)
        {
            if (Version.Major < 254)
            {
                return false;
            }
            return true;
        }
        return false;
    }
}

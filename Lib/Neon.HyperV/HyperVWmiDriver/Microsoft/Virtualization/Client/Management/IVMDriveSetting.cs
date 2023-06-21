namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_ResourceAllocationSettingData", PrimaryMapping = false)]
internal interface IVMDriveSetting : IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable, IDeleteableAsync, IDeleteable
{
    WmiObjectPath PhysicalPassThroughDrivePath { get; set; }

    string KSDConnectionPath { get; set; }

    IVMDriveControllerSetting ControllerSetting { get; set; }

    int ControllerAddress { get; set; }

    IVMBootEntry BootEntry { get; }

    IVirtualDiskSetting GetInsertedDisk();

    IVMHardDiskDrive GetPhysicalDrive();
}

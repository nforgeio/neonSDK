using System;

namespace Microsoft.Virtualization.Client.Management;

internal static class VMDeviceSettingTypeMapper
{
    internal static class ResourceType
    {
        public const int Unknown = 0;

        public const int Other = 1;

        public const int Memory = 4;

        public const int Processor = 3;

        public const int SerialPort = 21;

        public const int Mouse = 13;

        public const int Keyboard = 13;

        public const int DisketteController = 1;

        public const int GpuPartitionAdapter = 32770;

        public const int IdeController = 5;

        public const int ScsiSyntheticController = 6;

        public const int SerialController = 13;

        public const int DisplayController = 24;

        public const int PmemController = 32771;

        public const int DisketteDrive = 14;

        public const int DvdDrive = 16;

        public const int HardDiskDrive = 17;

        public const int LogicalUnitDrive = 32768;

        public const int LogicalDisk = 31;

        public const int EthernetPort = 10;

        public const int FibreChannelPort = 7;

        public const int FibreChannelConnection = 64764;

        public const int EthernetConnection = 33;

        public const int TPM = 34;

        public const int PciExpress = 32769;
    }

    internal static class ResourceSubType
    {
        public const string Memory = "Microsoft:Hyper-V:Memory";

        public const string Processor = "Microsoft:Hyper-V:Processor";

        public const string SerialPort = "Microsoft:Hyper-V:Serial Port";

        public const string SyntheticMouse = "Microsoft:Hyper-V:Synthetic Mouse";

        public const string SyntheticKeyboard = "Microsoft:Hyper-V:Synthetic Keyboard";

        public const string DisketteController = "Microsoft:Hyper-V:Virtual Diskette Controller";

        public const string GpuPartitionAdapter = "Microsoft:Hyper-V:GPU Partition";

        public const string IdeController = "Microsoft:Hyper-V:Emulated IDE Controller";

        public const string PmemController = "Microsoft:Hyper-V:Persistent Memory Controller";

        public const string ScsiSyntheticController = "Microsoft:Hyper-V:Synthetic SCSI Controller";

        public const string SerialController = "Microsoft:Hyper-V:Serial Controller";

        public const string S3DisplayController = "Microsoft:Hyper-V:S3 Display Controller";

        public const string Synthetic3DDisplayController = "Microsoft:Hyper-V:Synthetic 3D Display Controller";

        public const string SyntheticDisplayController = "Microsoft:Hyper-V:Synthetic Display Controller";

        public const string DisketteSyntheticDrive = "Microsoft:Hyper-V:Synthetic Diskette Drive";

        public const string DvdSyntheticDrive = "Microsoft:Hyper-V:Synthetic DVD Drive";

        public const string HardDiskSyntheticDrive = "Microsoft:Hyper-V:Synthetic Disk Drive";

        public const string HardDiskPhysicalDrive = "Microsoft:Hyper-V:Physical Disk Drive";

        public const string KeyStorageDrive = "Microsoft:Hyper-V:Storage Logical Unit";

        public const string HardDisk = "Microsoft:Hyper-V:Virtual Hard Disk";

        public const string IsoDisk = "Microsoft:Hyper-V:Virtual CD/DVD Disk";

        public const string FloppyDisk = "Microsoft:Hyper-V:Virtual Floppy Disk";

        public const string EthernetPortEmulated = "Microsoft:Hyper-V:Emulated Ethernet Port";

        public const string EthernetPortSynthetic = "Microsoft:Hyper-V:Synthetic Ethernet Port";

        public const string FibreChannelPort = "Microsoft:Hyper-V:Synthetic FibreChannel Port";

        public const string FibreChannelConnection = "Microsoft:Hyper-V:FibreChannel Connection";

        public const string EthernetConnection = "Microsoft:Hyper-V:Ethernet Connection";

        public const string TPM = "Microsoft:Hyper-V:TPM";

        public const string PciExpress = "Microsoft:Hyper-V:Virtual Pci Express Port";

        public const string Battery = "Microsoft:Hyper-V:Virtual Battery";
    }

    public static Type MapVMDeviceSettingTypeToObjectModelType(VMDeviceSettingType deviceType)
    {
        switch (deviceType)
        {
        case VMDeviceSettingType.IdeController:
        case VMDeviceSettingType.DisketteController:
            return typeof(IVMDriveControllerSetting);
        case VMDeviceSettingType.ScsiSyntheticController:
            return typeof(IVMScsiControllerSetting);
        case VMDeviceSettingType.PmemController:
            return typeof(IVMPmemControllerSetting);
        case VMDeviceSettingType.DisketteSyntheticDrive:
        case VMDeviceSettingType.HardDiskSyntheticDrive:
        case VMDeviceSettingType.HardDiskPhysicalDrive:
        case VMDeviceSettingType.DvdSyntheticDrive:
        case VMDeviceSettingType.KeyStorageDrive:
            return typeof(IVMDriveSetting);
        case VMDeviceSettingType.Memory:
            return typeof(IVMMemorySetting);
        case VMDeviceSettingType.Processor:
            return typeof(IVMProcessorSetting);
        case VMDeviceSettingType.SerialController:
            return typeof(IVMSerialControllerSetting);
        case VMDeviceSettingType.SerialPort:
            return typeof(IVMSerialPortSetting);
        case VMDeviceSettingType.EthernetPortEmulated:
            return typeof(IEmulatedEthernetPortSetting);
        case VMDeviceSettingType.EthernetPortSynthetic:
            return typeof(ISyntheticEthernetPortSetting);
        case VMDeviceSettingType.EthernetConnection:
            return typeof(IEthernetConnectionAllocationRequest);
        case VMDeviceSettingType.FibreChannelPort:
            return typeof(IFibreChannelPortSetting);
        case VMDeviceSettingType.FibreChannelConnection:
            return typeof(IFcPoolAllocationSetting);
        case VMDeviceSettingType.GpuPartition:
            return typeof(IVMGpuPartitionAdapterSetting);
        case VMDeviceSettingType.HardDisk:
        case VMDeviceSettingType.IsoDisk:
        case VMDeviceSettingType.FloppyDisk:
            return typeof(IVirtualDiskSetting);
        case VMDeviceSettingType.S3Video:
            return typeof(IVMS3DisplayControllerSetting);
        case VMDeviceSettingType.SynthVideo:
            return typeof(IVMSyntheticDisplayControllerSetting);
        case VMDeviceSettingType.SynthKeyboard:
            return typeof(IVMSyntheticKeyboardControllerSetting);
        case VMDeviceSettingType.SynthMouse:
            return typeof(IVMSyntheticMouseControllerSetting);
        case VMDeviceSettingType.PciExpress:
            return typeof(IVMAssignableDeviceSetting);
        case VMDeviceSettingType.Battery:
            return typeof(IVMBatterySetting);
        default:
            return typeof(IVMDeviceSetting);
        }
    }

    public static Type MapResourcePoolTypeToObjectModelType(VMDeviceSettingType deviceType)
    {
        switch (deviceType)
        {
        case VMDeviceSettingType.Processor:
            return typeof(IProcessorResourcePool);
        case VMDeviceSettingType.FibreChannelConnection:
            return typeof(IFibreChannelResourcePool);
        case VMDeviceSettingType.HardDisk:
        case VMDeviceSettingType.IsoDisk:
        case VMDeviceSettingType.FloppyDisk:
            return typeof(IVirtualDiskResourcePool);
        case VMDeviceSettingType.Synth3dVideo:
            return typeof(ISynth3dVideoResourcePool);
        case VMDeviceSettingType.EthernetConnection:
            return typeof(IEthernetConnectionResourcePool);
        case VMDeviceSettingType.PciExpress:
            return typeof(IPciExpressResourcePool);
        case VMDeviceSettingType.GpuPartition:
            return typeof(IGpuPartitionResourcePool);
        default:
            return typeof(IResourcePool);
        }
    }

    public static VMDeviceSettingType MapResourceSubTypeToVMDeviceSettingType(int resourceType, string resourceSubType, string otherResourceType)
    {
        VMDeviceSettingType result = VMDeviceSettingType.Unknown;
        if (resourceType == 1)
        {
            resourceSubType = otherResourceType;
        }
        switch (resourceSubType)
        {
        case "Microsoft:Hyper-V:Memory":
            result = VMDeviceSettingType.Memory;
            break;
        case "Microsoft:Hyper-V:Persistent Memory Controller":
            result = VMDeviceSettingType.PmemController;
            break;
        case "Microsoft:Hyper-V:Processor":
            result = VMDeviceSettingType.Processor;
            break;
        case "Microsoft:Hyper-V:Serial Port":
            result = VMDeviceSettingType.SerialPort;
            break;
        case "Microsoft:Hyper-V:Synthetic SCSI Controller":
            result = VMDeviceSettingType.ScsiSyntheticController;
            break;
        case "Microsoft:Hyper-V:Emulated IDE Controller":
            result = VMDeviceSettingType.IdeController;
            break;
        case "Microsoft:Hyper-V:Virtual Diskette Controller":
            result = VMDeviceSettingType.DisketteController;
            break;
        case "Microsoft:Hyper-V:Synthetic Disk Drive":
            result = VMDeviceSettingType.HardDiskSyntheticDrive;
            break;
        case "Microsoft:Hyper-V:Synthetic Diskette Drive":
            result = VMDeviceSettingType.DisketteSyntheticDrive;
            break;
        case "Microsoft:Hyper-V:Synthetic DVD Drive":
            result = VMDeviceSettingType.DvdSyntheticDrive;
            break;
        case "Microsoft:Hyper-V:GPU Partition":
            result = VMDeviceSettingType.GpuPartition;
            break;
        case "Microsoft:Hyper-V:Physical Disk Drive":
            result = VMDeviceSettingType.HardDiskPhysicalDrive;
            break;
        case "Microsoft:Hyper-V:Storage Logical Unit":
            result = VMDeviceSettingType.KeyStorageDrive;
            break;
        case "Microsoft:Hyper-V:Virtual Hard Disk":
            result = VMDeviceSettingType.HardDisk;
            break;
        case "Microsoft:Hyper-V:Virtual CD/DVD Disk":
            result = VMDeviceSettingType.IsoDisk;
            break;
        case "Microsoft:Hyper-V:Virtual Floppy Disk":
            result = VMDeviceSettingType.FloppyDisk;
            break;
        case "Microsoft:Hyper-V:Serial Controller":
            result = VMDeviceSettingType.SerialController;
            break;
        case "Microsoft:Hyper-V:Emulated Ethernet Port":
            result = VMDeviceSettingType.EthernetPortEmulated;
            break;
        case "Microsoft:Hyper-V:Synthetic Ethernet Port":
            result = VMDeviceSettingType.EthernetPortSynthetic;
            break;
        case "Microsoft:Hyper-V:Ethernet Connection":
            result = VMDeviceSettingType.EthernetConnection;
            break;
        case "Microsoft:Hyper-V:Synthetic FibreChannel Port":
            result = VMDeviceSettingType.FibreChannelPort;
            break;
        case "Microsoft:Hyper-V:FibreChannel Connection":
            result = VMDeviceSettingType.FibreChannelConnection;
            break;
        case "Microsoft:Hyper-V:Synthetic Display Controller":
            result = VMDeviceSettingType.SynthVideo;
            break;
        case "Microsoft:Hyper-V:Synthetic Keyboard":
            result = VMDeviceSettingType.SynthKeyboard;
            break;
        case "Microsoft:Hyper-V:Synthetic Mouse":
            result = VMDeviceSettingType.SynthMouse;
            break;
        case "Microsoft:Hyper-V:S3 Display Controller":
            result = VMDeviceSettingType.S3Video;
            break;
        case "Microsoft:Hyper-V:Synthetic 3D Display Controller":
            result = VMDeviceSettingType.Synth3dVideo;
            break;
        case "Microsoft:Hyper-V:Virtual Pci Express Port":
            result = VMDeviceSettingType.PciExpress;
            break;
        case "Microsoft:Hyper-V:Virtual Battery":
            result = VMDeviceSettingType.Battery;
            break;
        }
        return result;
    }

    public static void MapVMDeviceSettingTypeToResourceType(VMDeviceSettingType deviceType, out int resourceType, out string resourceSubType)
    {
        resourceType = 1;
        resourceSubType = string.Empty;
        switch (deviceType)
        {
        case VMDeviceSettingType.Memory:
            resourceType = 4;
            resourceSubType = "Microsoft:Hyper-V:Memory";
            break;
        case VMDeviceSettingType.PmemController:
            resourceType = 32771;
            resourceSubType = "Microsoft:Hyper-V:Persistent Memory Controller";
            break;
        case VMDeviceSettingType.Processor:
            resourceType = 3;
            resourceSubType = "Microsoft:Hyper-V:Processor";
            break;
        case VMDeviceSettingType.ScsiSyntheticController:
            resourceType = 6;
            resourceSubType = "Microsoft:Hyper-V:Synthetic SCSI Controller";
            break;
        case VMDeviceSettingType.IdeController:
            resourceType = 5;
            resourceSubType = "Microsoft:Hyper-V:Emulated IDE Controller";
            break;
        case VMDeviceSettingType.DisketteController:
            resourceType = 1;
            resourceSubType = "Microsoft:Hyper-V:Virtual Diskette Controller";
            break;
        case VMDeviceSettingType.GpuPartition:
            resourceType = 32770;
            resourceSubType = "Microsoft:Hyper-V:GPU Partition";
            break;
        case VMDeviceSettingType.HardDiskSyntheticDrive:
            resourceType = 17;
            resourceSubType = "Microsoft:Hyper-V:Synthetic Disk Drive";
            break;
        case VMDeviceSettingType.DisketteSyntheticDrive:
            resourceType = 14;
            resourceSubType = "Microsoft:Hyper-V:Synthetic Diskette Drive";
            break;
        case VMDeviceSettingType.DvdSyntheticDrive:
            resourceType = 16;
            resourceSubType = "Microsoft:Hyper-V:Synthetic DVD Drive";
            break;
        case VMDeviceSettingType.HardDiskPhysicalDrive:
            resourceType = 17;
            resourceSubType = "Microsoft:Hyper-V:Physical Disk Drive";
            break;
        case VMDeviceSettingType.KeyStorageDrive:
            resourceType = 32768;
            resourceSubType = "Microsoft:Hyper-V:Storage Logical Unit";
            break;
        case VMDeviceSettingType.HardDisk:
            resourceType = 31;
            resourceSubType = "Microsoft:Hyper-V:Virtual Hard Disk";
            break;
        case VMDeviceSettingType.IsoDisk:
            resourceType = 31;
            resourceSubType = "Microsoft:Hyper-V:Virtual CD/DVD Disk";
            break;
        case VMDeviceSettingType.FloppyDisk:
            resourceType = 31;
            resourceSubType = "Microsoft:Hyper-V:Virtual Floppy Disk";
            break;
        case VMDeviceSettingType.SerialController:
            resourceType = 13;
            resourceSubType = "Microsoft:Hyper-V:Serial Controller";
            break;
        case VMDeviceSettingType.SerialPort:
            resourceType = 21;
            break;
        case VMDeviceSettingType.EthernetPortEmulated:
            resourceType = 10;
            resourceSubType = "Microsoft:Hyper-V:Emulated Ethernet Port";
            break;
        case VMDeviceSettingType.EthernetPortSynthetic:
            resourceType = 10;
            resourceSubType = "Microsoft:Hyper-V:Synthetic Ethernet Port";
            break;
        case VMDeviceSettingType.EthernetConnection:
            resourceType = 33;
            resourceSubType = "Microsoft:Hyper-V:Ethernet Connection";
            break;
        case VMDeviceSettingType.FibreChannelPort:
            resourceType = 7;
            resourceSubType = "Microsoft:Hyper-V:Synthetic FibreChannel Port";
            break;
        case VMDeviceSettingType.FibreChannelConnection:
            resourceType = 64764;
            resourceSubType = "Microsoft:Hyper-V:FibreChannel Connection";
            break;
        case VMDeviceSettingType.Synth3dVideo:
            resourceType = 24;
            resourceSubType = "Microsoft:Hyper-V:Synthetic 3D Display Controller";
            break;
        case VMDeviceSettingType.SynthVideo:
            resourceType = 24;
            resourceSubType = "Microsoft:Hyper-V:Synthetic Display Controller";
            break;
        case VMDeviceSettingType.SynthKeyboard:
            resourceType = 13;
            resourceSubType = "Microsoft:Hyper-V:Synthetic Keyboard";
            break;
        case VMDeviceSettingType.SynthMouse:
            resourceType = 13;
            resourceSubType = "Microsoft:Hyper-V:Synthetic Mouse";
            break;
        case VMDeviceSettingType.S3Video:
            resourceType = 24;
            resourceSubType = "Microsoft:Hyper-V:S3 Display Controller";
            break;
        case VMDeviceSettingType.PciExpress:
            resourceType = 32769;
            resourceSubType = "Microsoft:Hyper-V:Virtual Pci Express Port";
            break;
        case VMDeviceSettingType.Battery:
            resourceType = 1;
            resourceSubType = "Microsoft:Hyper-V:Virtual Battery";
            break;
        default:
            throw new ArgumentException(null, "deviceType");
        }
    }
}

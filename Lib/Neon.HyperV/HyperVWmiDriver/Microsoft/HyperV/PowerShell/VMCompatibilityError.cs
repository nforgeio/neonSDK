using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMCompatibilityError
{
    public string Message { get; private set; }

    public long MessageId { get; private set; }

    public object Source { get; private set; }

    internal VMCompatibilityError(VirtualMachine pvm, MsvmError error)
    {
        Message = error.Message;
        MessageId = error.Id;
        WmiObjectPath errorSource = error.ErrorSource;
        if (!(errorSource != null) || !ObjectLocator.TryGetVirtualizationManagementObject<IVirtualizationManagementObject>(error.Server, errorSource, out var virtManObj))
        {
            return;
        }
        if (virtManObj is IVMComputerSystemBase)
        {
            Source = new VirtualMachine((IVMComputerSystemBase)virtManObj);
        }
        else if (virtManObj is IVMComputerSystemSetting)
        {
            IVMComputerSystemSetting iVMComputerSystemSetting = (IVMComputerSystemSetting)virtManObj;
            if (iVMComputerSystemSetting.IsSnapshot)
            {
                Source = new VMSnapshot(iVMComputerSystemSetting, pvm);
            }
            else
            {
                Source = new VirtualMachine(iVMComputerSystemSetting.VMComputerSystem);
            }
        }
        else
        {
            if (!(virtManObj is IVMDeviceSetting))
            {
                return;
            }
            IVMDeviceSetting obj = (IVMDeviceSetting)virtManObj;
            IVMComputerSystemSetting virtualComputerSystemSetting = obj.VirtualComputerSystemSetting;
            VirtualMachineBase virtualMachineBase = (virtualComputerSystemSetting.IsSnapshot ? ((VirtualMachineBase)new VMSnapshot(virtualComputerSystemSetting, pvm)) : ((VirtualMachineBase)pvm));
            switch (obj.VMDeviceSettingType)
            {
            case VMDeviceSettingType.Processor:
                Source = new VMProcessor((IVMProcessorSetting)virtManObj, virtualMachineBase);
                break;
            case VMDeviceSettingType.Memory:
                Source = new VMMemory((IVMMemorySetting)virtManObj, virtualMachineBase);
                break;
            case VMDeviceSettingType.EthernetConnection:
            {
                IEthernetConnectionAllocationRequest ethernetConnectionAllocationRequest = (IEthernetConnectionAllocationRequest)virtManObj;
                Source = new VMNetworkAdapter(ethernetConnectionAllocationRequest.Parent, ethernetConnectionAllocationRequest, virtualMachineBase);
                break;
            }
            case VMDeviceSettingType.SerialPort:
                Source = new VMComPort((IVMSerialPortSetting)virtManObj, virtualMachineBase);
                break;
            case VMDeviceSettingType.DvdSyntheticDrive:
            {
                IVMDriveSetting iVMDriveSetting2 = (IVMDriveSetting)virtManObj;
                Source = new DvdDrive(iVMDriveSetting2, iVMDriveSetting2.GetInsertedDisk(), FindControllerFromDrive(iVMDriveSetting2, virtualMachineBase), virtualMachineBase);
                break;
            }
            case VMDeviceSettingType.HardDiskSyntheticDrive:
            case VMDeviceSettingType.HardDiskPhysicalDrive:
            {
                IVMDriveSetting iVMDriveSetting = (IVMDriveSetting)virtManObj;
                Source = new HardDiskDrive(iVMDriveSetting, iVMDriveSetting.GetInsertedDisk(), FindControllerFromDrive(iVMDriveSetting, virtualMachineBase), virtualMachineBase);
                break;
            }
            case VMDeviceSettingType.HardDisk:
            {
                IVirtualDiskSetting virtualDiskSetting = (IVirtualDiskSetting)virtManObj;
                if (virtualDiskSetting.DriveSetting.ControllerSetting.VMDeviceSettingType == VMDeviceSettingType.PmemController)
                {
                    Source = HardDiskDrive.GetHardDiskDrive(virtualDiskSetting);
                }
                else
                {
                    Source = new HardDiskDrive(virtualDiskSetting.DriveSetting, virtualDiskSetting, FindControllerFromDrive(virtualDiskSetting.DriveSetting, virtualMachineBase), virtualMachineBase);
                }
                break;
            }
            case VMDeviceSettingType.FibreChannelConnection:
            {
                IFibreChannelPortSetting fibreChannelPortSetting = (IFibreChannelPortSetting)virtManObj;
                Source = new VMFibreChannelHba(fibreChannelPortSetting, fibreChannelPortSetting.GetConnectionConfiguration(), virtualMachineBase);
                break;
            }
            default:
                Source = null;
                break;
            }
        }
    }

    private static VMDriveController FindControllerFromDrive(IVMDriveSetting drive, VirtualMachineBase vm)
    {
        VMDriveController vMDriveController = null;
        IVMDriveControllerSetting controllerSetting = drive.ControllerSetting;
        if (controllerSetting is IVMScsiControllerSetting)
        {
            vMDriveController = vm.FindScsiControllerById(drive.ControllerSetting.DeviceId);
        }
        else
        {
            string address = controllerSetting.Address;
            if (!string.IsNullOrEmpty(address) && int.TryParse(address, out var result) && (result == 0 || result == 1))
            {
                vMDriveController = vm.GetIdeControllers()[result];
            }
        }
        if (vMDriveController == null)
        {
            throw new ObjectNotFoundException();
        }
        return vMDriveController;
    }
}

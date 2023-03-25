using System;
using Microsoft.HyperV.PowerShell.Common;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal abstract class DriveConfigurationData
{
	public VirtualMachineBase Parent { get; private set; }

	public AttachedDiskType AttachedDiskType { get; internal set; }

	public VMDriveController Controller { get; internal set; }

	public int? ControllerLocation { get; internal set; }

	public string VirtualDiskPath { get; internal set; }

	public string ResourcePoolName { get; internal set; }

	internal abstract VMDeviceSettingType DriveSettingType { get; }

	internal abstract VMDeviceSettingType DiskSettingType { get; }

	internal abstract bool HasAttachedDisk { get; }

	internal abstract string DescriptionForDriveAdd { get; }

	internal abstract string DescriptionForDiskAttach { get; }

	protected DriveConfigurationData(VirtualMachineBase parent)
	{
		Parent = parent;
	}

	internal virtual void CopyValuesToDriveSetting(IVMDriveSetting settingToModify)
	{
		if (Controller != null)
		{
			settingToModify.ControllerSetting = Controller.GetControllerSetting();
		}
		if (ControllerLocation.HasValue)
		{
			settingToModify.ControllerAddress = ControllerLocation.Value;
		}
	}

	internal virtual void CopyValuesToDiskSetting(IVirtualDiskSetting settingToModify)
	{
		settingToModify.Path = VirtualDiskPath;
		settingToModify.PoolId = (VMResourcePool.IsPrimordialPoolName(ResourcePoolName) ? string.Empty : ResourcePoolName);
	}

	internal IVMDriveSetting CreateTemplateDriveSetting()
	{
		IVMDriveSetting iVMDriveSetting = CreateTemplateDeviceSetting<IVMDriveSetting>(DriveSettingType);
		CopyValuesToDriveSetting(iVMDriveSetting);
		return iVMDriveSetting;
	}

	internal IVirtualDiskSetting CreateTemplateDiskSetting()
	{
		IVirtualDiskSetting virtualDiskSetting = CreateTemplateDeviceSetting<IVirtualDiskSetting>(DiskSettingType);
		CopyValuesToDiskSetting(virtualDiskSetting);
		return virtualDiskSetting;
	}

	private TDeviceSetting CreateTemplateDeviceSetting<TDeviceSetting>(VMDeviceSettingType deviceType) where TDeviceSetting : IVMDeviceSetting
	{
		return (TDeviceSetting)ObjectLocator.GetHostComputerSystem(Parent.Server).GetSettingCapabilities(deviceType, Capabilities.DefaultCapability);
	}

	private bool IsControllerMoveRequired(DriveConfigurationData oldConfiguration)
	{
		if (ControllerLocation != oldConfiguration.ControllerLocation)
		{
			return true;
		}
		return !string.Equals(Controller.DeviceID, oldConfiguration.Controller.DeviceID, StringComparison.OrdinalIgnoreCase);
	}

	internal virtual bool IsRemoveThenAddRequired(DriveConfigurationData oldConfiguration)
	{
		return IsControllerMoveRequired(oldConfiguration);
	}

	internal bool RemovesAttachedDisk(DriveConfigurationData originalConfiguration)
	{
		if (originalConfiguration == null)
		{
			throw new ArgumentNullException("originalConfiguration");
		}
		if (originalConfiguration.HasAttachedDisk)
		{
			return !HasAttachedDisk;
		}
		return false;
	}

	internal bool AddsAttachedDisk(DriveConfigurationData originalConfiguration)
	{
		if (originalConfiguration == null)
		{
			throw new ArgumentNullException("originalConfiguration");
		}
		if (!originalConfiguration.HasAttachedDisk)
		{
			return HasAttachedDisk;
		}
		return false;
	}

	internal bool ModifiesAttachedDisk(DriveConfigurationData originalConfiguration)
	{
		if (originalConfiguration == null)
		{
			throw new ArgumentNullException("originalConfiguration");
		}
		if (originalConfiguration.HasAttachedDisk)
		{
			return HasAttachedDisk;
		}
		return false;
	}

	private IVMDriveSetting AddDriveSetting(IVMDriveSetting templateDriveSetting, IOperationWatcher operationWatcher)
	{
		return ((VirtualMachine)Parent).AddDeviceSetting(templateDriveSetting, DescriptionForDriveAdd, operationWatcher);
	}

	internal IVirtualDiskSetting AttachDiskSettingToDrive(IVMDriveSetting targetDriveSetting, IOperationWatcher operationWatcher)
	{
		VirtualMachine obj = (VirtualMachine)Parent;
		IVirtualDiskSetting virtualDiskSetting = CreateTemplateDiskSetting();
		virtualDiskSetting.DriveSetting = targetDriveSetting;
		IVirtualDiskSetting result = obj.AddDeviceSetting(virtualDiskSetting, DescriptionForDiskAttach, operationWatcher);
		targetDriveSetting.InvalidateAssociationCache();
		return result;
	}

	private void RollBackAddedDriveSetting(IVMDriveSetting driveSetting, IOperationWatcher operationWatcher)
	{
		try
		{
			VirtualMachine.RemoveDeviceSetting(driveSetting, TaskDescriptions.DriveConfigurationRollback, Parent, operationWatcher);
		}
		catch (Exception innerException)
		{
			ExceptionHelper.DisplayErrorOnException(ExceptionHelper.CreateRollbackFailedException(innerException), operationWatcher);
		}
	}

	internal Tuple<IVMDriveSetting, IVirtualDiskSetting> AddDriveInternal(IOperationWatcher operationWatcher)
	{
		IVMDriveSetting templateDriveSetting = CreateTemplateDriveSetting();
		IVMDriveSetting iVMDriveSetting = AddDriveSetting(templateDriveSetting, operationWatcher);
		IVirtualDiskSetting item = null;
		try
		{
			if (HasAttachedDisk)
			{
				item = AttachDiskSettingToDrive(iVMDriveSetting, operationWatcher);
			}
		}
		catch
		{
			RollBackAddedDriveSetting(iVMDriveSetting, operationWatcher);
			throw;
		}
		Controller.GetControllerSetting().InvalidateAssociationCache();
		return Tuple.Create(iVMDriveSetting, item);
	}

	internal Drive AddDrive(IOperationWatcher operationWatcher)
	{
		Tuple<IVMDriveSetting, IVirtualDiskSetting> tuple = AddDriveInternal(operationWatcher);
		IVMDriveSetting item = tuple.Item1;
		_ = tuple.Item2;
		return Drive.CreateForExistingDrive(item, Controller, Parent);
	}
}

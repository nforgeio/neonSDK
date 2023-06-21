using System;
using System.Globalization;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal abstract class Drive : DriveBase, IRemovable, IBootableDevice
{
    private VMDriveController m_Controller;

    public int ControllerLocation => m_DriveSetting.GetData(UpdatePolicy.EnsureUpdated).ControllerAddress;

    public int ControllerNumber => m_Controller.ControllerNumber;

    public ControllerType ControllerType => m_Controller.ControllerType;

    [VirtualizationObjectIdentifier(IdentifierFlags.FriendlyName)]
    public override string Name
    {
        get
        {
            return string.Format(CultureInfo.CurrentCulture, "{0} on {1} controller number {2} at location {3}", base.Name, ControllerType, ControllerNumber, ControllerLocation);
        }
        internal set
        {
            base.Name = value;
        }
    }

    VMBootSource IBootableDevice.BootSource => new VMBootSource(m_DriveSetting.GetData(UpdatePolicy.EnsureAssociatorsUpdated).BootEntry, GetParentAs<VirtualMachineBase>());

    protected abstract string DescriptionForDriveRemove { get; }

    internal Drive(IVMDriveSetting setting, IVirtualDiskSetting attachedDiskSetting, VMDriveController driveController, VirtualMachineBase parentVirtualMachineObject)
        : base(setting, attachedDiskSetting, parentVirtualMachineObject)
    {
        m_Controller = driveController;
    }

    void IRemovable.Remove(IOperationWatcher operationWatcher)
    {
        RemoveSelf(operationWatcher);
    }

    internal virtual void RemoveSelf(IOperationWatcher operationWatcher)
    {
        bool flag = false;
        IVirtualDiskSetting virtualDiskSetting = null;
        if (!m_AttachedDiskSetting.IsDeleted)
        {
            virtualDiskSetting = m_AttachedDiskSetting.GetData(UpdatePolicy.None);
            if (virtualDiskSetting != null)
            {
                RemoveAttachedDiskSetting(operationWatcher);
                flag = true;
            }
        }
        try
        {
            RemoveDriveSetting(operationWatcher);
        }
        catch
        {
            if (flag)
            {
                RollBackRemovedDiskSetting(virtualDiskSetting, operationWatcher);
            }
            throw;
        }
        m_Controller.GetControllerSetting().InvalidateAssociationCache();
        GetParentAs<VirtualMachineBase>().InvalidateDeviceSettingsList();
    }

    private void RemoveDriveSetting(IOperationWatcher operationWatcher)
    {
        operationWatcher.PerformDelete(m_DriveSetting.GetData(UpdatePolicy.None), DescriptionForDriveRemove, this);
    }

    internal override DriveConfigurationData GetCurrentConfiguration()
    {
        DriveConfigurationData currentConfiguration = base.GetCurrentConfiguration();
        currentConfiguration.Controller = m_Controller;
        currentConfiguration.ControllerLocation = ControllerLocation;
        return currentConfiguration;
    }

    internal override void PerformRemoveThenAdd(DriveConfigurationData newConfiguration, DriveConfigurationData oldConfiguration, IOperationWatcher operationWatcher)
    {
        bool flag = false;
        try
        {
            ((IRemovable)this).Remove(operationWatcher);
            flag = true;
            Tuple<IVMDriveSetting, IVirtualDiskSetting> tuple = newConfiguration.AddDriveInternal(operationWatcher);
            m_DriveSetting = InitializePrimaryDataUpdater(tuple.Item1);
            m_AttachedDiskSetting = InitializeDiskUpdater(tuple.Item2);
            m_Controller = newConfiguration.Controller;
        }
        catch
        {
            if (flag)
            {
                try
                {
                    Tuple<IVMDriveSetting, IVirtualDiskSetting> tuple2 = oldConfiguration.AddDriveInternal(operationWatcher);
                    m_DriveSetting = InitializePrimaryDataUpdater(tuple2.Item1);
                    m_AttachedDiskSetting = InitializeDiskUpdater(tuple2.Item2);
                    m_Controller = oldConfiguration.Controller;
                }
                catch (Exception innerException)
                {
                    ExceptionHelper.DisplayErrorOnException(ExceptionHelper.CreateRollbackFailedException(innerException), operationWatcher);
                }
            }
            throw;
        }
    }

    internal static Drive CreateForExistingDrive(IVMDriveSetting driveSetting, VMDriveController driveController, VirtualMachineBase parent)
    {
        VMDeviceSettingType vMDeviceSettingType = driveSetting.VMDeviceSettingType;
        IVirtualDiskSetting insertedDisk = driveSetting.GetInsertedDisk();
        switch (vMDeviceSettingType)
        {
        case VMDeviceSettingType.HardDiskSyntheticDrive:
        case VMDeviceSettingType.HardDiskPhysicalDrive:
            return new HardDiskDrive(driveSetting, insertedDisk, driveController, parent);
        case VMDeviceSettingType.KeyStorageDrive:
            return new KeyStorageDrive(driveSetting, insertedDisk, driveController, parent);
        default:
            return new DvdDrive(driveSetting, insertedDisk, driveController, parent);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.HyperV.PowerShell.Common;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class HardDiskDrive : Drive
{
    public override string Path
    {
        get
        {
            IVMDriveSetting data = m_DriveSetting.GetData(UpdatePolicy.EnsureUpdated);
            if (data.VMDeviceSettingType == VMDeviceSettingType.HardDiskPhysicalDrive)
            {
                try
                {
                    return data.GetPhysicalDrive().FriendlyName;
                }
                catch (ObjectNotFoundException)
                {
                    return null;
                }
            }
            return base.Path;
        }
    }

    public int? DiskNumber
    {
        get
        {
            int? result = null;
            IVMDriveSetting data = m_DriveSetting.GetData(UpdatePolicy.EnsureUpdated);
            if (data.VMDeviceSettingType == VMDeviceSettingType.HardDiskPhysicalDrive)
            {
                try
                {
                    uint? physicalDiskNumber = data.GetPhysicalDrive().PhysicalDiskNumber;
                    if (physicalDiskNumber.HasValue)
                    {
                        return NumberConverter.UInt32ToInt32(physicalDiskNumber.Value);
                    }
                    return result;
                }
                catch (ObjectNotFoundException)
                {
                    return null;
                }
            }
            return result;
        }
    }

    public ulong? MaximumIOPS => m_AttachedDiskSetting.GetData(UpdatePolicy.EnsureUpdated)?.MaximumIOPS;

    public ulong? MinimumIOPS => m_AttachedDiskSetting.GetData(UpdatePolicy.EnsureUpdated)?.MinimumIOPS;

    public Guid? QoSPolicyID => m_AttachedDiskSetting.GetData(UpdatePolicy.EnsureUpdated)?.StorageQoSPolicyID;

    public bool? SupportPersistentReservations => m_AttachedDiskSetting.GetData(UpdatePolicy.EnsureUpdated)?.PersistentReservationsSupported;

    public CacheAttributes? WriteHardeningMethod => (CacheAttributes?)m_AttachedDiskSetting.GetData(UpdatePolicy.EnsureUpdated)?.WriteHardeningMethod;

    internal AttachedDiskType AttachedDiskType
    {
        get
        {
            if (m_DriveSetting.GetData(UpdatePolicy.None).VMDeviceSettingType == VMDeviceSettingType.HardDiskPhysicalDrive)
            {
                return AttachedDiskType.Physical;
            }
            return (m_AttachedDiskSetting.GetData(UpdatePolicy.EnsureUpdated) != null) ? AttachedDiskType.Virtual : AttachedDiskType.None;
        }
    }

    internal override string PutDescription => TaskDescriptions.SetVMHardDiskDrive;

    protected override string DescriptionForDiskDetach => TaskDescriptions.SetVMHardDiskDrive_DetachVirtualDisk;

    protected override string DescriptionForDriveRemove => TaskDescriptions.RemoveVMHardDiskDrive;

    internal HardDiskDrive(IVMDriveSetting setting, IVirtualDiskSetting attachedDiskSetting, VMDriveController driveController, VirtualMachineBase parentVirtualMachineObject)
        : base(setting, attachedDiskSetting, driveController, parentVirtualMachineObject)
    {
    }

    internal IReadOnlyCollection<IMetricValue> GetMetricValues()
    {
        return m_AttachedDiskSetting.GetData(UpdatePolicy.EnsureAssociatorsUpdated)?.GetMetricValues();
    }

    internal static HardDiskDrive GetHardDiskDrive(IVirtualDiskSetting diskSetting)
    {
        IVMDriveSetting driveSetting = diskSetting.DriveSetting;
        IVMDriveControllerSetting controllerSetting = driveSetting.ControllerSetting;
        VirtualMachine virtualMachine = new VirtualMachine(controllerSetting.VirtualComputerSystemSetting.VMComputerSystem);
        VMDriveController driveController = virtualMachine.GetDriveControllers().Single((VMDriveController controller) => string.Equals(controller.DeviceID, controllerSetting.DeviceId, StringComparison.OrdinalIgnoreCase));
        return new HardDiskDrive(driveSetting, diskSetting, driveController, virtualMachine);
    }

    internal IVirtualDiskSetting GetVirtualDiskSetting(UpdatePolicy policy)
    {
        return m_AttachedDiskSetting.GetData(policy);
    }

    internal bool IsMovable()
    {
        DataExchangeComponent dataExchangeComponent = GetParentAs<VirtualMachineBase>().GetDataExchangeComponent();
        string unmovableVhdTag = GetUnmovableVhdTag();
        if (dataExchangeComponent != null)
        {
            return !dataExchangeComponent.GetHostOnlyKeyValuePairItems().Any((DataExchangeItem dataItem) => string.Equals(dataItem.Name, unmovableVhdTag, StringComparison.OrdinalIgnoreCase));
        }
        return true;
    }

    protected override DriveConfigurationData GetCurrentConfigurationSelf()
    {
        HardDriveConfigurationData hardDriveConfigurationData = new HardDriveConfigurationData(GetParentAs<VirtualMachineBase>());
        IVMDriveSetting data = m_DriveSetting.GetData(UpdatePolicy.EnsureAssociatorsUpdated);
        hardDriveConfigurationData.AttachedDiskType = AttachedDiskType;
        hardDriveConfigurationData.PhysicalDrivePath = data.PhysicalPassThroughDrivePath;
        hardDriveConfigurationData.MaximumIOPS = MaximumIOPS;
        hardDriveConfigurationData.MinimumIOPS = MinimumIOPS;
        hardDriveConfigurationData.QoSPolicyID = QoSPolicyID;
        hardDriveConfigurationData.SupportPersistentReservations = SupportPersistentReservations;
        hardDriveConfigurationData.WriteHardeningMethod = WriteHardeningMethod;
        return hardDriveConfigurationData;
    }

    internal override void RemoveSelf(IOperationWatcher operationWatcher)
    {
        IVMComputerSystem computerSystemAs = GetParentAs<VirtualMachineBase>().GetVirtualMachine().GetComputerSystemAs<IVMComputerSystem>(UpdatePolicy.None);
        string unmovableVhdTag = GetUnmovableVhdTag();
        base.RemoveSelf(operationWatcher);
        computerSystemAs?.RemoveKvpItem(unmovableVhdTag, KvpItemPool.Internal);
    }

    private string GetUnmovableVhdTag()
    {
        return string.Format(format: (base.ControllerType == ControllerType.IDE) ? "DisableVhdMoveIDE_{0}_Location_{1}" : ((base.ControllerType != ControllerType.PMEM) ? "DisableVhdMoveSCSI_{0}_Location_{1}" : "DisableVhdMovePMEM_{0}_Location_{1}"), provider: CultureInfo.InvariantCulture, arg0: base.ControllerNumber, arg1: base.ControllerLocation);
    }
}

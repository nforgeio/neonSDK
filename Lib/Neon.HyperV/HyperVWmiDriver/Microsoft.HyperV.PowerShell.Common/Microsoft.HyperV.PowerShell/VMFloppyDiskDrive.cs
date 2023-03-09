using System;
using Microsoft.HyperV.PowerShell.Common;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMFloppyDiskDrive : DriveBase
{
	internal override string PutDescription => TaskDescriptions.SetVMFloppyDiskDrive;

	protected override string DescriptionForDiskDetach => TaskDescriptions.SetVMFloppyDiskDrive_EjectDisk;

	internal VMFloppyDiskDrive(IVMDriveSetting setting, IVirtualDiskSetting attachedDiskSetting, VirtualMachineBase parentComputeResource)
		: base(setting, attachedDiskSetting, parentComputeResource)
	{
	}

	protected override DriveConfigurationData GetCurrentConfigurationSelf()
	{
		FloppyDriveConfigurationData floppyDriveConfigurationData = new FloppyDriveConfigurationData(GetParentAs<VirtualMachineBase>());
		IVirtualDiskSetting data = m_AttachedDiskSetting.GetData(UpdatePolicy.EnsureUpdated);
		if (!m_AttachedDiskSetting.IsDeleted && data != null)
		{
			floppyDriveConfigurationData.AttachedDiskType = AttachedDiskType.Virtual;
			floppyDriveConfigurationData.VirtualDiskPath = data.Path;
			floppyDriveConfigurationData.ResourcePoolName = data.PoolId;
		}
		else
		{
			floppyDriveConfigurationData.AttachedDiskType = AttachedDiskType.None;
		}
		return floppyDriveConfigurationData;
	}

	internal override void PerformRemoveThenAdd(DriveConfigurationData newConfiguration, DriveConfigurationData oldConfiguration, IOperationWatcher operationWatcher)
	{
		throw new NotImplementedException();
	}
}

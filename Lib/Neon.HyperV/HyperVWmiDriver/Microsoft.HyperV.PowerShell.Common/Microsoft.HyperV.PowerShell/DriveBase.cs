using System;
using Microsoft.HyperV.PowerShell.Common;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal abstract class DriveBase : VMDevice
{
	internal IDataUpdater<IVMDriveSetting> m_DriveSetting;

	internal IDataUpdater<IVirtualDiskSetting> m_AttachedDiskSetting;

	public virtual string Path => m_AttachedDiskSetting.GetData(UpdatePolicy.EnsureUpdated)?.Path;

	public string PoolName
	{
		get
		{
			string text = null;
			IVirtualDiskSetting data = m_AttachedDiskSetting.GetData(UpdatePolicy.EnsureUpdated);
			if (data != null)
			{
				text = data.PoolId;
				if (string.IsNullOrEmpty(text))
				{
					text = "Primordial";
				}
			}
			return text;
		}
	}

	protected abstract string DescriptionForDiskDetach { get; }

	internal DriveBase(IVMDriveSetting setting, IVirtualDiskSetting attachedDiskSetting, VirtualMachineBase parentVirtualMachineObject)
		: base(setting, parentVirtualMachineObject)
	{
		m_DriveSetting = InitializePrimaryDataUpdater(setting);
		m_AttachedDiskSetting = InitializeDiskUpdater(attachedDiskSetting);
	}

	protected IDataUpdater<IVirtualDiskSetting> InitializeDiskUpdater(IVirtualDiskSetting diskSetting)
	{
		return new DependentObjectDataUpdater<IVirtualDiskSetting>(diskSetting, ReloadAttachedDisk);
	}

	private IVirtualDiskSetting ReloadAttachedDisk(TimeSpan threshold)
	{
		return m_DriveSetting.GetData(UpdatePolicy.EnsureAssociatorsUpdated).GetInsertedDisk();
	}

	internal void RollBackRemovedDiskSetting(IVirtualDiskSetting templateDiskSetting, IOperationWatcher operationWatcher)
	{
		try
		{
			templateDiskSetting.DriveSetting = m_DriveSetting.GetData(UpdatePolicy.None);
			IVirtualDiskSetting diskSetting = GetParentAs<VirtualMachine>().AddDeviceSetting(templateDiskSetting, TaskDescriptions.DriveConfigurationRollback, operationWatcher);
			m_AttachedDiskSetting = InitializeDiskUpdater(diskSetting);
		}
		catch (Exception innerException)
		{
			ExceptionHelper.DisplayErrorOnException(ExceptionHelper.CreateRollbackFailedException(innerException), operationWatcher);
		}
	}

	internal void RemoveAttachedDiskSetting(IOperationWatcher operationWatcher)
	{
		IVirtualDiskSetting data = m_AttachedDiskSetting.GetData(UpdatePolicy.None);
		operationWatcher.PerformDelete(data, DescriptionForDiskDetach, this);
		m_AttachedDiskSetting = InitializeDiskUpdater(null);
	}

	internal void Configure(DriveConfigurationData newConfiguration, DriveConfigurationData originalConfiguration, IOperationWatcher operationWatcher)
	{
		if (newConfiguration.IsRemoveThenAddRequired(originalConfiguration))
		{
			PerformRemoveThenAdd(newConfiguration, originalConfiguration, operationWatcher);
		}
		else if (newConfiguration.RemovesAttachedDisk(originalConfiguration))
		{
			RemoveAttachedDiskSetting(operationWatcher);
		}
		else if (newConfiguration.AddsAttachedDisk(originalConfiguration))
		{
			IVMDriveSetting data = m_DriveSetting.GetData(UpdatePolicy.None);
			IVirtualDiskSetting diskSetting = newConfiguration.AttachDiskSettingToDrive(data, operationWatcher);
			m_AttachedDiskSetting = InitializeDiskUpdater(diskSetting);
		}
		else if (newConfiguration.ModifiesAttachedDisk(originalConfiguration))
		{
			IVirtualDiskSetting data2 = m_AttachedDiskSetting.GetData(UpdatePolicy.None);
			newConfiguration.CopyValuesToDiskSetting(data2);
			PutOneDeviceSetting(data2, operationWatcher);
		}
	}

	internal override IDataUpdater<IVMDeviceSetting> GetDeviceDataUpdater()
	{
		return m_DriveSetting;
	}

	protected abstract DriveConfigurationData GetCurrentConfigurationSelf();

	internal virtual DriveConfigurationData GetCurrentConfiguration()
	{
		DriveConfigurationData currentConfigurationSelf = GetCurrentConfigurationSelf();
		if (currentConfigurationSelf.AttachedDiskType == AttachedDiskType.Virtual)
		{
			currentConfigurationSelf.VirtualDiskPath = Path;
			currentConfigurationSelf.ResourcePoolName = PoolName;
		}
		return currentConfigurationSelf;
	}

	internal abstract void PerformRemoveThenAdd(DriveConfigurationData newConfiguration, DriveConfigurationData oldConfiguration, IOperationWatcher operationWatcher);
}

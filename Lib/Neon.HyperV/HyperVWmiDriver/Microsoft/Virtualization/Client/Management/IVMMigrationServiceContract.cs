using System;
using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IVMMigrationServiceContract : IVMMigrationService, IVirtualizationManagementObject, IPutable
{
	public IVMMigrationServiceSetting Setting => null;

	public IVMMigrationCapabilities Capabilities => null;

	public abstract Server Server { get; }

	public abstract WmiObjectPath ManagementPath { get; }

	public event EventHandler Deleted;

	public event EventHandler CacheUpdated;

	public IVMMigrationSetting GetMigrationSetting(VMMigrationType migrationType)
	{
		return null;
	}

	public string[] GetMigrationServiceListenerIPAddressList()
	{
		return null;
	}

	public IVMTask BeginMigration(IVMComputerSystem computerSystem, string destinationHost, IVMMigrationSetting migrationSetting, IVMComputerSystemSetting newSystemSettingData, List<IVirtualDiskSetting> newResourceSettingData)
	{
		return null;
	}

	public void EndMigration(IVMTask task)
	{
	}

	public IVMTask BeginCheckMigratability(IVMComputerSystem computerSystem, string destinationHost, IVMMigrationSetting migrationSetting, IVMComputerSystemSetting newSystemSettingData, List<IVirtualDiskSetting> newResourceSettingData)
	{
		return null;
	}

	public void EndCheckMigratability(IVMTask task)
	{
	}

	public void AddNetworkSettings(string[] networkSettings)
	{
	}

	public void ModifyNetworkSettings(string[] networkSettings)
	{
	}

	public void RemoveNetworkSettings(WmiObjectPath[] networkSettingPaths)
	{
	}

	public abstract void Put();

	public abstract void InvalidatePropertyCache();

	public abstract void UpdatePropertyCache();

	public abstract void UpdatePropertyCache(TimeSpan threshold);

	public abstract void RegisterForInstanceModificationEvents(InstanceModificationEventStrategy strategy);

	public abstract void UnregisterForInstanceModificationEvents();

	public abstract void InvalidateAssociationCache();

	public abstract void UpdateAssociationCache();

	public abstract void UpdateAssociationCache(TimeSpan threshold);

	public abstract string GetEmbeddedInstance();

	public abstract void DiscardPendingPropertyChanges();
}

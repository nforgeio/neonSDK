using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_VirtualSystemMigrationService")]
internal interface IVMMigrationService : IVirtualizationManagementObject, IPutable
{
	IVMMigrationServiceSetting Setting { get; }

	IVMMigrationCapabilities Capabilities { get; }

	IVMMigrationSetting GetMigrationSetting(VMMigrationType migrationType);

	string[] GetMigrationServiceListenerIPAddressList();

	IVMTask BeginMigration(IVMComputerSystem computerSystem, string destinationHost, IVMMigrationSetting migrationSetting, IVMComputerSystemSetting newSystemSettingData, List<IVirtualDiskSetting> newResourceSettingData);

	void EndMigration(IVMTask task);

	IVMTask BeginCheckMigratability(IVMComputerSystem computerSystem, string destinationHost, IVMMigrationSetting migrationSetting, IVMComputerSystemSetting newSystemSettingData, List<IVirtualDiskSetting> newResourceSettingData);

	void EndCheckMigratability(IVMTask task);

	void AddNetworkSettings(string[] networkSettings);

	void ModifyNetworkSettings(string[] networkSettings);

	void RemoveNetworkSettings(WmiObjectPath[] networkSettingPaths);
}

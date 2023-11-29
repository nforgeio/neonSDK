#define TRACE
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Microsoft.Virtualization.Client.Management;

internal class VMMigrationServiceView : View, IVMMigrationService, IVirtualizationManagementObject, IPutable
{
    internal static class WmiMemberNames
    {
        public const string MigrationServiceListenerIPAddressList = "MigrationServiceListenerIPAddressList";

        public const string MigrateVirtualSystemToHost = "MigrateVirtualSystemToHost";

        public const string CheckSystemMigratabilityInfo = "CheckVirtualSystemIsMigratable";

        public const string AddNetworkSettings = "AddNetworkSettings";

        public const string ModifyNetworkSettings = "ModifyNetworkSettings";

        public const string ModifyServiceSettings = "ModifyServiceSettings";

        public const string RemoveNetworkSettings = "RemoveNetworkSettings";
    }

    public IVMMigrationCapabilities Capabilities => GetRelatedObject<IVMMigrationCapabilities>(base.Associations.ElementCapabilities);

    public IVMMigrationServiceSetting Setting => GetRelatedObject<IVMMigrationServiceSetting>(base.Associations.ElementSettingData);

    public IVMMigrationSetting GetMigrationSetting(VMMigrationType migrationType)
    {
        return Capabilities.MigrationSettings.First((IVMMigrationSetting setting) => setting.MigrationType == migrationType);
    }

    public string[] GetMigrationServiceListenerIPAddressList()
    {
        return GetProperty<string[]>("MigrationServiceListenerIPAddressList");
    }

    private IVMTask BeginMigrationOperation(bool performCheck, IVMComputerSystem computerSystem, string destinationHost, IVMMigrationSetting migrationSetting, IVMComputerSystemSetting newSystemSettingData, List<IVirtualDiskSetting> newResourceSettingData)
    {
        if (computerSystem == null)
        {
            throw new ArgumentNullException("computerSystem");
        }
        if (migrationSetting == null)
        {
            throw new ArgumentNullException("migrationSetting");
        }
        string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.MigrationOperationFailed, computerSystem.Name);
        VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Starting migration operation on '{0}'", computerSystem.ManagementPath));
        string embeddedInstance = migrationSetting.GetEmbeddedInstance();
        string text = null;
        if (newSystemSettingData != null)
        {
            text = newSystemSettingData.GetEmbeddedInstance();
        }
        string[] array = null;
        if (newResourceSettingData != null && newResourceSettingData.Count > 0)
        {
            array = new string[newResourceSettingData.Count];
            int num = 0;
            foreach (IVirtualDiskSetting newResourceSettingDatum in newResourceSettingData)
            {
                array[num] = newResourceSettingDatum.GetEmbeddedInstance();
                num++;
            }
        }
        object[] array2 = new object[6] { computerSystem, destinationHost, embeddedInstance, text, array, null };
        uint result = InvokeMethod(performCheck ? "CheckVirtualSystemIsMigratable" : "MigrateVirtualSystemToHost", array2);
        IVMTask iVMTask = BeginMethodTaskReturn(result, null, array2[5]);
        iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
        return iVMTask;
    }

    public IVMTask BeginMigration(IVMComputerSystem computerSystem, string destinationHost, IVMMigrationSetting migrationSetting, IVMComputerSystemSetting newSystemSettingData, List<IVirtualDiskSetting> newResourceSettingData)
    {
        return BeginMigrationOperation(performCheck: false, computerSystem, destinationHost, migrationSetting, newSystemSettingData, newResourceSettingData);
    }

    public void EndMigration(IVMTask task)
    {
        EndMethod(task, VirtualizationOperation.MigrationOperation);
        VMTrace.TraceUserActionCompleted("Migration completed successfully.");
    }

    public IVMTask BeginCheckMigratability(IVMComputerSystem computerSystem, string destinationHost, IVMMigrationSetting migrationSetting, IVMComputerSystemSetting newSystemSettingData, List<IVirtualDiskSetting> newResourceSettingData)
    {
        return BeginMigrationOperation(performCheck: true, computerSystem, destinationHost, migrationSetting, newSystemSettingData, newResourceSettingData);
    }

    public void EndCheckMigratability(IVMTask task)
    {
        EndMethod(task, VirtualizationOperation.MigrationOperation);
        VMTrace.TraceUserActionCompleted("Migratability check completed successfully.");
    }

    public void AddNetworkSettings(string[] networkSettings)
    {
        if (networkSettings.Length == 0)
        {
            return;
        }
        object[] array = new object[2] { networkSettings, null };
        string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.AddMigrationNetworkSettingsFailed, ToString());
        uint result = InvokeMethod("AddNetworkSettings", array);
        using IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[1]);
        if (iVMTask.Status != VMTaskStatus.CompletedSuccessfully)
        {
            iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
            VMTrace.TraceUserActionCompleted("Adding migration network settings failed.");
        }
        EndMethod(iVMTask, VirtualizationOperation.AddMigrationNetworkSettings);
    }

    public void ModifyNetworkSettings(string[] networkSettings)
    {
        if (networkSettings.Length == 0)
        {
            return;
        }
        object[] array = new object[2] { networkSettings, null };
        string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.ModifyMigrationNetworkSettingsFailed, ToString());
        uint result = InvokeMethod("ModifyNetworkSettings", array);
        using IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[1]);
        if (iVMTask.Status != VMTaskStatus.CompletedSuccessfully)
        {
            iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
            VMTrace.TraceUserActionCompleted("Modifying migration network settings failed.");
        }
        EndMethod(iVMTask, VirtualizationOperation.ModifyMigrationNetworkSettings);
    }

    public void RemoveNetworkSettings(WmiObjectPath[] networkSettings)
    {
        if (networkSettings.Length == 0)
        {
            return;
        }
        object[] array = new object[2] { networkSettings, null };
        string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.RemoveMigrationNetworkSettingsFailed, ToString());
        uint result = InvokeMethod("RemoveNetworkSettings", array);
        using IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[1]);
        if (iVMTask.Status != VMTaskStatus.CompletedSuccessfully)
        {
            iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
            VMTrace.TraceUserActionCompleted("Modifying migration network settings failed.");
        }
        EndMethod(iVMTask, VirtualizationOperation.ModifyMigrationNetworkSettings);
    }
}

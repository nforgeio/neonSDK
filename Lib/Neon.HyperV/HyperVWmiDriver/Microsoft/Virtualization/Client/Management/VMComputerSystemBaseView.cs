#define TRACE
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Microsoft.Virtualization.Client.Management;

internal class VMComputerSystemBaseView : View, IVMComputerSystemBase, IVirtualizationManagementObject, IDeleteableAsync, IDeleteable
{
    internal static class WmiMemberNames
    {
        public const string Name = "ElementName";

        public const string CreatedTime = "InstallDate";

        public const string TimeOfLastStateChange = "TimeOfLastStateChange";

        public const string InstanceId = "Name";

        public const string State = "EnabledState";

        public const string OtherState = "OtherEnabledState";

        public const string HealthState = "HealthState";

        public const string ReplicationMode = "ReplicationMode";

        public const string OperationalStatus = "OperationalStatus";

        public const string StatusDescriptions = "StatusDescriptions";

        public const string TimeOfLastConfigurationChange = "TimeOfLastConfigurationChange";

        public const string EnhancedSessionModeState = "EnhancedSessionModeState";

        public const string NumberOfNumaNodes = "NumberOfNumaNodes";

        public const string HwThreadsPerCoreRealized = "HwThreadsPerCoreRealized";

        public const string AddSystemComponentSettings = "AddSystemComponentSettings";

        public const string AddDevices = "AddResourceSettings";

        public const string TakeSnapshot = "CreateSnapshot";

        public const string Delete = "DestroySystem";

        public const string RequestStateChange = "RequestStateChange";

        public const string RequestReplicationStateChangeEx = "RequestReplicationStateChangeEx";

        public const string GetReplicationStatisticsEx = "GetReplicationStatisticsEx";

        public const string RemoveKvpItems = "RemoveKvpItems";

        public const string InjectNonMaskableInterrupt = "InjectNonMaskableInterrupt";

        public const string UpgradeSystemVersion = "UpgradeSystemVersion";
    }

    protected class VMComputerSystemErrorCodeMapper : ErrorCodeMapper
    {
        private readonly string m_VMName;

        private readonly string m_CreatedDeviceName;

        public VMComputerSystemErrorCodeMapper(string vmName, string createdDeviceName)
        {
            m_VMName = vmName;
            m_CreatedDeviceName = createdDeviceName;
        }

        public override string MapError(VirtualizationOperation operation, long errorCode, string operationFailedMsg)
        {
            if (operation == VirtualizationOperation.AddDevice && errorCode == -2)
            {
                return string.Format(CultureInfo.CurrentCulture, ErrorMessages.AddDeviceSucceededButObjectPathNotFound, m_CreatedDeviceName, m_VMName);
            }
            return base.MapError(operation, errorCode, operationFailedMsg);
        }
    }

    protected SnapshotCreatedEventHandler m_SnapshotCreated;

    protected EventObjectKey m_SnapshotCreationKey;

    protected string m_DeviceToAddName;

    protected string m_InstanceId;

    public string Name => GetProperty<string>("ElementName");

    public DateTime TimeOfLastStateChange => GetProperty<DateTime>("TimeOfLastStateChange");

    public string InstanceId => m_InstanceId;

    public VMComputerSystemState State
    {
        get
        {
            VMComputerSystemState vMComputerSystemState = (VMComputerSystemState)GetProperty<ushort>("EnabledState");
            if (vMComputerSystemState == VMComputerSystemState.Other)
            {
                vMComputerSystemState = VMComputerSystemStateUtilities.ConvertVMComputerSystemOtherState(GetProperty<string>("OtherEnabledState"));
            }
            if (!VMComputerSystemStateUtilities.IsVMComputerSystemStateValid(vMComputerSystemState))
            {
                throw ThrowHelper.CreateInvalidPropertyValueException("EnabledState", typeof(VMComputerSystemState), vMComputerSystemState, null);
            }
            return vMComputerSystemState;
        }
    }

    public VMComputerSystemHealthState HealthState
    {
        get
        {
            VMComputerSystemHealthState vMComputerSystemHealthState = (VMComputerSystemHealthState)GetProperty<ushort>("HealthState");
            if (!VMComputerSystemStateUtilities.IsVMComputerSystemHealthStateValid(vMComputerSystemHealthState))
            {
                vMComputerSystemHealthState = VMComputerSystemHealthState.Unknown;
            }
            return vMComputerSystemHealthState;
        }
    }

    protected EventObjectKey SnapshotCreationKey => m_SnapshotCreationKey ?? (m_SnapshotCreationKey = ObjectKeyCreator.CreateSnapshotCreationEventObjectKey(base.Key.Server, InstanceId));

    public int NumberOfNumaNodes => NumberConverter.UInt16ToInt32(GetProperty<ushort>("NumberOfNumaNodes"));

    public int? HwThreadsPerCore
    {
        get
        {
            uint? propertyOrDefault = GetPropertyOrDefault<uint?>("HwThreadsPerCoreRealized");
            if (propertyOrDefault.HasValue)
            {
                return NumberConverter.UInt32ToInt32(propertyOrDefault.Value);
            }
            return null;
        }
    }

    public DateTime TimeOfLastConfigurationChange => GetProperty<DateTime>("TimeOfLastConfigurationChange");

    public IEnumerable<IVMMemory> Memory => GetRelatedObjects<IVMMemory>(base.Associations.VirtualMachineMemory);

    public IVMSecurityInformation SecurityInformation => GetRelatedObject<IVMSecurityInformation>(base.Associations.VirtualMachineSecurityInformation, throwIfNotFound: false);

    public IVMKeyboard Keyboard => GetRelatedObject<IVMKeyboard>(base.Associations.VirtualMachineKeyboard, throwIfNotFound: false);

    public IVMShutdownComponent ShutdownComponent => GetRelatedObject<IVMShutdownComponent>(base.Associations.VirtualMachineShutDownComponent, throwIfNotFound: false);

    public IVMVssComponent VssComponent => GetRelatedObject<IVMVssComponent>(base.Associations.VirtualMachineVssComponent, throwIfNotFound: false);

    public IVMComputerSystemSetting Setting
    {
        get
        {
            IVMComputerSystemSetting relatedObject = GetRelatedObject<IVMComputerSystemSetting>(base.Associations.SystemToSystemSetting, throwIfNotFound: false);
            if (relatedObject == null)
            {
                base.Proxy.UpdateOneCachedAssociation(base.Associations.SystemToSystemSetting, TimeSpan.Zero);
                relatedObject = GetRelatedObject<IVMComputerSystemSetting>(base.Associations.SystemToSystemSetting);
            }
            return relatedObject;
        }
    }

    public IVMExportSetting ExportSetting => GetRelatedObject<IVMExportSetting>(base.Associations.SystemToExportSettingData, throwIfNotFound: false);

    public IEnumerable<IVMComputerSystemSetting> Snapshots => GetRelatedObjects<IVMComputerSystemSetting>(base.Associations.SystemToSnapshotSetting);

    public IEnumerable<IVMComputerSystemSetting> ReplicaSnapshots => new List<IVMComputerSystemSetting>(GetRelatedObjects<IVMComputerSystemSetting>(base.Associations.SystemToSnapshotSetting)).FindAll((IVMComputerSystemSetting systemSetting) => systemSetting.VirtualSystemType == VirtualSystemType.ReplicaSnapshot || systemSetting.VirtualSystemType == VirtualSystemType.ApplicationConsistentReplicaSnapshot || systemSetting.VirtualSystemType == VirtualSystemType.ReplicaSnapshotWithSyncedData);

    public IEnumerable<IVMTask> Tasks => GetRelatedObjects<IVMTask>(base.Associations.AffectedJobElement);

    public int NumberOfSnapshots
    {
        get
        {
            using IEnumerator<IVMComputerSystemSetting> enumerator = Snapshots.GetEnumerator();
            int num = 0;
            while (enumerator.MoveNext())
            {
                num++;
            }
            return num;
        }
    }

    public IHostComputerSystem HostSystem => GetRelatedObject<IHostComputerSystem>(base.Associations.HostedDependency);

    public IVMReplicationSettingData VMReplicationSettingData => GetReplicationSettingData(ReplicationRelationshipType.Primary, throwIfNotFound: true);

    public IVMReplicationSettingData VMExtendedReplicationSettingData => GetReplicationSettingData(ReplicationRelationshipType.Extended, throwIfNotFound: true);

    public IVMReplicationRelationship VMReplicationRelationship => GetReplicationRelationship(ReplicationRelationshipType.Primary, throwIfNotFound: false);

    public IVMReplicationRelationship VMExtendedReplicationRelationship => GetReplicationRelationship(ReplicationRelationshipType.Extended, throwIfNotFound: false);

    internal override void Initialize(IProxy proxy, ObjectKey key)
    {
        base.Initialize(proxy, key);
        m_InstanceId = (string)key.ManagementPath.KeyValues["Name"];
    }

    public VMComputerSystemOperationalStatus[] GetOperationalStatus()
    {
        return VMComputerSystemStateUtilities.ConvertOperationalStatus(GetProperty<ushort[]>("OperationalStatus"));
    }

    public string[] GetStatusDescriptions()
    {
        return GetProperty<string[]>("StatusDescriptions");
    }

    protected override ErrorCodeMapper GetErrorCodeMapper()
    {
        return new VMComputerSystemErrorCodeMapper(Name, m_DeviceToAddName);
    }

    public IVMTask BeginAddDevice(IVMDeviceSetting deviceToAdd)
    {
        if (deviceToAdd == null)
        {
            throw new ArgumentNullException("deviceToAdd");
        }
        IProxy serviceProxy = GetServiceProxy();
        object[] array = new object[4]
        {
            Setting,
            new string[1] { deviceToAdd.GetEmbeddedInstance() },
            null,
            null
        };
        string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.AddDeviceFailed, deviceToAdd.DeviceTypeName, Name);
        m_DeviceToAddName = deviceToAdd.FriendlyName;
        VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Starting adding device of type '{0}' into virtual machine '{1}' ('{2}')", deviceToAdd.DeviceTypeName, InstanceId, Name), deviceToAdd.GetEmbeddedInstance());
        uint result = serviceProxy.InvokeMethod("AddResourceSettings", array);
        object[] array2 = array[2] as object[];
        object affectedElementPath = null;
        if (array2 != null && array2.Length != 0)
        {
            affectedElementPath = array2[0];
        }
        IVMTask iVMTask = BeginMethodTaskReturn(result, affectedElementPath, array[3]);
        iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
        return iVMTask;
    }

    public IVMDeviceSetting EndAddDevice(IVMTask addDeviceTask)
    {
        IVMDeviceSetting iVMDeviceSetting = EndMethodReturn<IVMDeviceSetting>(addDeviceTask, VirtualizationOperation.AddDevice);
        VMTrace.TraceUserActionCompleted("Adding device completed successfully.");
        if (iVMDeviceSetting != null)
        {
            try
            {
                iVMDeviceSetting.UpdatePropertyCache(View.gm_NewObjectUpdateTime);
                iVMDeviceSetting.UpdateAssociationCache(View.gm_NewObjectUpdateTime);
                return iVMDeviceSetting;
            }
            catch (VirtualizationManagementException ex)
            {
                VMTrace.TraceWarning("Unable to update newly added device.", ex);
                return iVMDeviceSetting;
            }
        }
        return iVMDeviceSetting;
    }

    public IVMTask BeginSetState(VMComputerSystemState state)
    {
        string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.SetStateFailed, Name);
        object[] array = new object[3]
        {
            (ushort)state,
            null,
            null
        };
        VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Changing virtual machine '{0}' ('{1}') state from '{2}' to '{3}'", InstanceId, Name, State, state));
        uint result = InvokeMethod("RequestStateChange", array);
        IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[1]);
        iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
        return iVMTask;
    }

    public void EndSetState(IVMTask setStateTask)
    {
        EndMethod(setStateTask, VirtualizationOperation.SetState);
        VMTrace.TraceUserActionCompleted(string.Format(CultureInfo.InvariantCulture, "State change completed successfully for virtual machine '{0}' ('{1}').", InstanceId, Name));
    }

    public IVMTask BeginDelete()
    {
        string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.DeleteVirtualMachineFailed, Name);
        IProxy serviceProxy = GetServiceProxy();
        object[] array = new object[2] { this, null };
        VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Deleting virtual machine '{0}' ('{1}').", InstanceId, Name));
        uint result = serviceProxy.InvokeMethod("DestroySystem", array);
        IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[1]);
        iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
        return iVMTask;
    }

    public void EndDelete(IVMTask deleteTask)
    {
        EndMethod(deleteTask, VirtualizationOperation.Delete);
        RemoveFromCache();
        VMTrace.TraceUserActionCompleted("Virtual machine deleted successfully.");
    }

    public void Delete()
    {
        using IVMTask iVMTask = BeginDelete();
        iVMTask.WaitForCompletion();
        EndDelete(iVMTask);
    }

    public void RemoveFromCache()
    {
        ObjectLocator.RemoveVirtualMachineFromCache(base.Proxy);
    }

    public IVMComputerSystemSetting GetPreviousSnapshot(bool needsRefresh)
    {
        if (needsRefresh)
        {
            base.Proxy.UpdateOneCachedAssociation(base.Associations.SystemToPreviousSnapshot, TimeSpan.Zero);
        }
        return GetRelatedObject<IVMComputerSystemSetting>(base.Associations.SystemToPreviousSnapshot, throwIfNotFound: false);
    }

    public IVMReplicationSettingData GetReplicationSettingData(ReplicationRelationshipType relationshipType, bool throwIfNotFound)
    {
        string instanceIdEnding = ((relationshipType == ReplicationRelationshipType.Primary) ? "0" : "1");
        IVMReplicationSettingData iVMReplicationSettingData = GetRelatedObjects<IVMReplicationSettingData>(base.Associations.SystemToReplicationSetting).FirstOrDefault((IVMReplicationSettingData settingData) => settingData.InstanceId.EndsWith(instanceIdEnding, StringComparison.OrdinalIgnoreCase));
        if (throwIfNotFound && iVMReplicationSettingData == null)
        {
            throw ThrowHelper.CreateRelatedObjectNotFoundException(base.Server, typeof(IVMReplicationSettingData));
        }
        return iVMReplicationSettingData;
    }

    public IVMReplicationRelationship GetReplicationRelationship(ReplicationRelationshipType relationshipType, bool throwIfNotFound)
    {
        string instanceIdEnding = ((relationshipType == ReplicationRelationshipType.Primary) ? "0" : "1");
        IVMReplicationRelationship iVMReplicationRelationship = GetRelatedObjects<IVMReplicationRelationship>(base.Associations.SystemToReplicationRelationship).FirstOrDefault((IVMReplicationRelationship relationshipData) => relationshipData.InstanceId.EndsWith(instanceIdEnding, StringComparison.OrdinalIgnoreCase));
        if (throwIfNotFound && iVMReplicationRelationship == null)
        {
            throw ThrowHelper.CreateRelatedObjectNotFoundException(base.Server, typeof(IVMReplicationRelationship));
        }
        return iVMReplicationRelationship;
    }
}

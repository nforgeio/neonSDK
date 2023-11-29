#define TRACE
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Microsoft.Virtualization.Client.Management;

internal class VMComputerSystemSettingView : View, IVMComputerSystemSetting, IVirtualizationManagementObject, IPutableAsync, IPutable, IDeleteableAsync, IDeleteable
{
    internal static class WmiMemberNames
    {
        public const string InstanceId = "InstanceID";

        public const string Name = "ElementName";

        public const string Version = "Version";

        public const string CreationTime = "CreationTime";

        public const string VirtualSystemType = "VirtualSystemType";

        public const string VirtualSystemSubType = "VirtualSystemSubType";

        public const string ConfigurationId = "ConfigurationID";

        public const string Notes = "Notes";

        public const string Description = "Description";

        public const string VirtualNumaEnabled = "VirtualNumaEnabled";

        public const string BiosNumLock = "BIOSNumLock";

        public const string BootOrder = "BootOrder";

        public const string BootSourceOrder = "BootSourceOrder";

        public const string SystemName = "VirtualSystemIdentifier";

        public const string ParentSnapshot = "Parent";

        public const string IsSaved = "IsSaved";

        public const string SecureBootEnabled = "SecureBootEnabled";

        public const string SecureBootTemplateId = "SecureBootTemplateId";

        public const string UserSnapshotType = "UserSnapshotType";

        public const string AutomaticCriticalErrorAction = "AutomaticCriticalErrorAction";

        public const string AutomaticCriticalErrorActionTimeout = "AutomaticCriticalErrorActionTimeout";

        public const string NetworkBootPreferredProtocol = "NetworkBootPreferredProtocol";

        public const string ConsoleMode = "ConsoleMode";

        public const string EnhancedSessionTransportType = "EnhancedSessionTransportType";

        public const string LockOnDisconnect = "LockOnDisconnect";

        public const string GuestControlledCacheTypes = "GuestControlledCacheTypes";

        public const string PauseAfterBootFailure = "PauseAfterBootFailure";

        public const string LowMemoryMappedIoSpace = "LowMmioGapSize";

        public const string HighMemoryMappedIoSpace = "HighMmioGapSize";

        public const string IsAutomaticCheckpoint = "IsAutomaticSnapshot";

        public const string EnableAutomaticCheckpoints = "AutomaticSnapshotsEnabled";

        public const string ConfigurationDataRoot = "ConfigurationDataRoot";

        public const string SnapshotDataRoot = "SnapshotDataRoot";

        public const string SwapFileDataRoot = "SwapFileDataRoot";

        public const string AutomaticStartupAction = "AutomaticStartupAction";

        public const string AutomaticShutdownAction = "AutomaticShutdownAction";

        public const string AutomaticStartupActionDelay = "AutomaticStartupActionDelay";

        public const string Put = "ModifySystemSettings";

        public const string ApplySnapShot = "ApplySnapshot";

        public const string Delete = "DestroySnapshot";

        public const string DeleteTree = "DestroySnapshotTree";

        public const string GetThumbnailImage = "GetVirtualSystemThumbnailImage";

        public const string GetSizeOfSystemFiles = "GetSizeOfSystemFiles";

        public const string ClearSnapshotState = "ClearSnapshotState";
    }

    internal static class WmiVirtualSystemTypeNames
    {
        public const string SnapshotPrefix = "Microsoft:Hyper-V:Snapshot:";

        public const string RealizedVM = "Microsoft:Hyper-V:System:Realized";

        public const string PlannedVM = "Microsoft:Hyper-V:System:Planned";

        public const string RealizedSnapshot = "Microsoft:Hyper-V:Snapshot:Realized";

        public const string RecoverySnapshot = "Microsoft:Hyper-V:Snapshot:Recovery";

        public const string PlannedSnapshot = "Microsoft:Hyper-V:Snapshot:Planned";

        public const string MissingSnapshot = "Microsoft:Hyper-V:Snapshot:Missing";

        public const string ReplicaStandardRecoverySnapshot = "Microsoft:Hyper-V:Snapshot:Replica:Standard";

        public const string ReplicaApplicationConsistentRecoverySnapshot = "Microsoft:Hyper-V:Snapshot:Replica:ApplicationConsistent";

        public const string ReplicaPlannedRecoverySnapshot = "Microsoft:Hyper-V:Snapshot:Replica:PlannedFailover";

        public const string ReplicaSettings = "Microsoft:Hyper-V:Replica";
    }

    internal static class WmiVirtualSystemSubTypeNames
    {
        public const string Type1 = "Microsoft:Hyper-V:SubType:1";

        public const string Type2 = "Microsoft:Hyper-V:SubType:2";
    }

    internal static class SecureBootTemplateGuids
    {
        public const string MicrosoftWindows = "1734c6e8-3154-4dda-ba5f-a874cc483422";
    }

    private IReadOnlyList<Association> m_AssociationsNotToUpdate;

    public string InstanceId => GetProperty<string>("InstanceID");

    public string SystemName => GetProperty<string>("VirtualSystemIdentifier");

    public string ConfigurationId => GetProperty<string>("ConfigurationID");

    public string Name
    {
        get
        {
            return GetProperty<string>("ElementName");
        }
        set
        {
            if (value == null)
            {
                value = string.Empty;
            }
            SetProperty("ElementName", value);
        }
    }

    public string Version
    {
        get
        {
            return GetProperty<string>("Version");
        }
        set
        {
            if (value == null)
            {
                value = string.Empty;
            }
            SetProperty("Version", value);
        }
    }

    public VirtualSystemType VirtualSystemType => WmiVirtualSystemTypeToEnumVirtualSystemType(GetProperty<string>("VirtualSystemType"));

    public VirtualSystemSubType VirtualSystemSubType
    {
        get
        {
            return WmiVirtualSystemSubTypeToEnumVirtualSystemSubType(GetProperty<string>("VirtualSystemSubType"));
        }
        set
        {
            SetProperty("VirtualSystemSubType", EnumVirtualSystemSubTypeToWmiVirtualSystemSubType(value));
        }
    }

    public bool IsSaved => GetProperty<bool?>("IsSaved").GetValueOrDefault();

    public bool IsSnapshot
    {
        get
        {
            if (VirtualSystemType != VirtualSystemType.RealizedVirtualMachine && VirtualSystemType != VirtualSystemType.PlannedVirtualMachine)
            {
                return VirtualSystemType != VirtualSystemType.ReplicaSettings;
            }
            return false;
        }
    }

    public bool LockOnDisconnect
    {
        get
        {
            return GetProperty<bool>("LockOnDisconnect");
        }
        set
        {
            SetProperty("LockOnDisconnect", value);
        }
    }

    public string Notes
    {
        get
        {
            string[] property = GetProperty<string[]>("Notes");
            if (property.Length >= 1)
            {
                return property[0];
            }
            return string.Empty;
        }
        set
        {
            if (value == null)
            {
                value = string.Empty;
            }
            SetProperty("Notes", new string[1] { value });
        }
    }

    public string Description => GetProperty<string>("Description");

    public DateTime CreationTime => GetProperty<DateTime>("CreationTime");

    public bool VirtualNumaEnabled
    {
        get
        {
            return GetProperty<bool>("VirtualNumaEnabled");
        }
        set
        {
            SetProperty("VirtualNumaEnabled", value);
        }
    }

    public bool BiosNumLock
    {
        get
        {
            return GetProperty<bool>("BIOSNumLock");
        }
        set
        {
            SetProperty("BIOSNumLock", value);
        }
    }

    public bool GuestControlledCacheTypes
    {
        get
        {
            return GetProperty<bool>("GuestControlledCacheTypes");
        }
        set
        {
            SetProperty("GuestControlledCacheTypes", value);
        }
    }

    public ushort NetworkBootPreferredProtocol
    {
        get
        {
            return GetProperty<ushort>("NetworkBootPreferredProtocol");
        }
        set
        {
            SetProperty("NetworkBootPreferredProtocol", value);
        }
    }

    public ConsoleModeType ConsoleMode
    {
        get
        {
            return (ConsoleModeType)GetPropertyOrDefault("ConsoleMode", (ushort)0);
        }
        set
        {
            SetProperty("ConsoleMode", (ushort)value);
        }
    }

    public EnhancedSessionTransportType EnhancedSessionTransportType
    {
        get
        {
            return (EnhancedSessionTransportType)GetPropertyOrDefault("EnhancedSessionTransportType", (ushort)0);
        }
        set
        {
            SetProperty("EnhancedSessionTransportType", (ushort)value);
        }
    }

    public IVMComputerSystemSetting ParentSnapshot
    {
        get
        {
            IVMComputerSystemSetting result = null;
            string property = GetProperty<string>("Parent");
            if (!string.IsNullOrEmpty(property))
            {
                result = (IVMComputerSystemSetting)GetViewFromPath(property);
            }
            return result;
        }
    }

    public IEnumerable<IVMComputerSystemSetting> ChildSettings => GetRelatedObjects<IVMComputerSystemSetting>(base.Associations.SnapshotSettingToChildSetting);

    public bool SecureBootEnabled
    {
        get
        {
            return GetProperty<bool>("SecureBootEnabled");
        }
        set
        {
            SetProperty("SecureBootEnabled", value);
        }
    }

    public Guid? SecureBootTemplateId
    {
        get
        {
            string propertyOrDefault = GetPropertyOrDefault("SecureBootTemplateId", "1734c6e8-3154-4dda-ba5f-a874cc483422");
            if (propertyOrDefault != null)
            {
                return new Guid(propertyOrDefault);
            }
            return null;
        }
        set
        {
            if (value.HasValue)
            {
                SetProperty("SecureBootTemplateId", value.Value.ToString("D"));
                return;
            }
            throw ThrowHelper.CreateInvalidPropertyValueException("SecureBootTemplateId", typeof(Guid), null, null);
        }
    }

    public bool PauseAfterBootFailure
    {
        get
        {
            return GetProperty<bool>("PauseAfterBootFailure");
        }
        set
        {
            SetProperty("PauseAfterBootFailure", value);
        }
    }

    public uint LowMemoryMappedIoSpace
    {
        get
        {
            return (uint)(GetPropertyOrDefault("LowMmioGapSize", 0uL) * 1048576);
        }
        set
        {
            SetProperty("LowMmioGapSize", ((value + 2097151) / 1048576u) & -2);
        }
    }

    public ulong HighMemoryMappedIoSpace
    {
        get
        {
            return GetPropertyOrDefault("HighMmioGapSize", 0uL) * 1048576;
        }
        set
        {
            SetProperty("HighMmioGapSize", ((value + 2097151) / 1048576uL) & 0xFFFFFFFFFFFFFFFEuL);
        }
    }

    public bool IsAutomaticCheckpoint => GetPropertyOrDefault("IsAutomaticSnapshot", defaultValue: false);

    public bool EnableAutomaticCheckpoints
    {
        get
        {
            return GetPropertyOrDefault("AutomaticSnapshotsEnabled", defaultValue: false);
        }
        set
        {
            SetProperty("AutomaticSnapshotsEnabled", value);
        }
    }

    public string ConfigurationDataRoot
    {
        get
        {
            return GetProperty<string>("ConfigurationDataRoot");
        }
        set
        {
            if (value == null)
            {
                value = string.Empty;
            }
            SetProperty("ConfigurationDataRoot", value);
        }
    }

    public string SnapshotDataRoot
    {
        get
        {
            return GetProperty<string>("SnapshotDataRoot");
        }
        set
        {
            if (value == null)
            {
                value = string.Empty;
            }
            SetProperty("SnapshotDataRoot", value);
        }
    }

    public UserSnapshotType UserSnapshotType
    {
        get
        {
            return (UserSnapshotType)GetProperty<ushort>("UserSnapshotType");
        }
        set
        {
            SetProperty("UserSnapshotType", (ushort)value);
        }
    }

    public string SwapFileDataRoot
    {
        get
        {
            return GetProperty<string>("SwapFileDataRoot");
        }
        set
        {
            if (value == null)
            {
                value = string.Empty;
            }
            SetProperty("SwapFileDataRoot", value);
        }
    }

    public ServiceStartOperation AutomaticStartupAction
    {
        get
        {
            ServiceStartOperation property = (ServiceStartOperation)GetProperty<ushort>("AutomaticStartupAction");
            if (!IsServiceStartOperationValueValid(property))
            {
                throw ThrowHelper.CreateInvalidPropertyValueException("AutomaticStartupAction", typeof(ServiceStartOperation), property, null);
            }
            return property;
        }
        set
        {
            SetProperty("AutomaticStartupAction", NumberConverter.Int32ToUInt16((int)value));
        }
    }

    public ServiceStopOperation AutomaticShutdownAction
    {
        get
        {
            ServiceStopOperation property = (ServiceStopOperation)GetProperty<ushort>("AutomaticShutdownAction");
            if (!IsServiceStopOperationValueValid(property))
            {
                throw ThrowHelper.CreateInvalidPropertyValueException("AutomaticShutdownAction", typeof(ServiceStartOperation), property, null);
            }
            return property;
        }
        set
        {
            SetProperty("AutomaticShutdownAction", NumberConverter.Int32ToUInt16((int)value));
        }
    }

    public TimeSpan AutomaticStartupActionDelay
    {
        get
        {
            return GetProperty<TimeSpan>("AutomaticStartupActionDelay");
        }
        set
        {
            SetProperty("AutomaticStartupActionDelay", value);
        }
    }

    public CriticalErrorAction AutomaticCriticalErrorAction
    {
        get
        {
            return (CriticalErrorAction)GetProperty<ushort>("AutomaticCriticalErrorAction");
        }
        set
        {
            SetProperty("AutomaticCriticalErrorAction", NumberConverter.Int32ToUInt16((int)value));
        }
    }

    public TimeSpan AutomaticCriticalErrorActionTimeout
    {
        get
        {
            return GetProperty<TimeSpan>("AutomaticCriticalErrorActionTimeout");
        }
        set
        {
            SetProperty("AutomaticCriticalErrorActionTimeout", value);
        }
    }

    public IVMComputerSystemBase VMComputerSystem => GetRelatedObject<IVMComputerSystemBase>((!IsSnapshot) ? base.Associations.SystemSettingToSystem : base.Associations.SnapshotSettingToSystem);

    protected override IReadOnlyList<Association> AssociationsNotToUpdate
    {
        get
        {
            IReadOnlyList<Association> readOnlyList = m_AssociationsNotToUpdate;
            if (readOnlyList == null)
            {
                Association[] obj = new Association[1] { base.Associations.SystemSettingSettingDataLimited };
                IReadOnlyList<Association> readOnlyList2 = obj;
                m_AssociationsNotToUpdate = obj;
                readOnlyList = readOnlyList2;
            }
            return readOnlyList;
        }
    }

    public override event EventHandler Deleted
    {
        add
        {
            base.Proxy.EventWatchAdditionalConditions = new string[1] { string.Format(CultureInfo.InvariantCulture, "{0}=\"{1}\"", "VirtualSystemType", GetProperty<string>("VirtualSystemType")) };
            base.Deleted += value;
        }
        remove
        {
            base.Deleted -= value;
        }
    }

    public override event EventHandler CacheUpdated
    {
        add
        {
            base.Proxy.EventWatchAdditionalConditions = new string[1] { string.Format(CultureInfo.InvariantCulture, "{0}=\"{1}\"", "VirtualSystemType", GetProperty<string>("VirtualSystemType")) };
            base.CacheUpdated += value;
        }
        remove
        {
            base.CacheUpdated -= value;
        }
    }

    public BootDevice[] GetBootOrder()
    {
        object property = GetProperty<object>("BootOrder");
        BootDevice[] array = null;
        if (property != null && property is ushort[] array2)
        {
            array = new BootDevice[array2.Length];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = (BootDevice)array2[i];
            }
        }
        return array;
    }

    public void SetBootOrder(BootDevice[] bootOrder)
    {
        ushort[] array = null;
        if (bootOrder != null)
        {
            array = new ushort[bootOrder.Length];
            for (int i = 0; i < bootOrder.Length; i++)
            {
                array[i] = (ushort)bootOrder[i];
            }
        }
        else
        {
            array = new ushort[0];
        }
        SetProperty("BootOrder", array);
    }

    public IEnumerable<IVMBootEntry> GetFirmwareBootOrder()
    {
        string[] property = GetProperty<string[]>("BootSourceOrder");
        if (property == null)
        {
            return Enumerable.Empty<IVMBootEntry>();
        }
        return property.Select((string bootEntryPath) => GetViewFromPath(bootEntryPath)).Cast<IVMBootEntry>();
    }

    public void SetFirmwareBootOrder(IEnumerable<IVMBootEntry> bootEntries)
    {
        string[] value = ((bootEntries != null) ? bootEntries.Select((IVMBootEntry bootEntry) => bootEntry.ManagementPath.ToString()).ToArray() : new string[0]);
        SetProperty("BootSourceOrder", value);
    }

    public IVMTask BeginApply()
    {
        IProxy snapshotServiceProxy = GetSnapshotServiceProxy();
        if (!(VMComputerSystem is IVMComputerSystem iVMComputerSystem))
        {
            throw new InvalidOperationException();
        }
        string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.ApplySnapshotFailed, Name, iVMComputerSystem.Name);
        object[] array = new object[2] { this, null };
        VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Applying snapshot '{0}' ('{1}') onto virtual machine '{2}' ('{3}')", InstanceId, Name, iVMComputerSystem.InstanceId, iVMComputerSystem.Name));
        uint result = snapshotServiceProxy.InvokeMethod("ApplySnapshot", array);
        IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[1]);
        iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
        return iVMTask;
    }

    public void EndApply(IVMTask applyTask)
    {
        EndMethod(applyTask, VirtualizationOperation.ApplySnapshot);
        VMTrace.TraceUserActionCompleted("Applying snapshot completed successfully.");
    }

    public void Apply()
    {
        using IVMTask iVMTask = BeginApply();
        iVMTask.WaitForCompletion();
        EndApply(iVMTask);
    }

    public IVMTask BeginDelete()
    {
        string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.DeleteVirtualSystemSettingFailed, Name);
        IProxy snapshotServiceProxy = GetSnapshotServiceProxy();
        object[] array = new object[2] { this, null };
        VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Deleting snapshot '{0}' ('{1}')", InstanceId, Name));
        uint result = snapshotServiceProxy.InvokeMethod("DestroySnapshot", array);
        IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[1]);
        iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
        return iVMTask;
    }

    public void EndDelete(IVMTask deleteTask)
    {
        EndMethod(deleteTask, VirtualizationOperation.Delete);
        VMTrace.TraceUserActionCompleted("Deleting snapshot completed successfully.");
    }

    public void Delete()
    {
        using IVMTask iVMTask = BeginDelete();
        iVMTask.WaitForCompletion();
        EndDelete(iVMTask);
    }

    public IVMTask BeginDeleteTree()
    {
        string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.DeleteVirtualSystemSettingTreeFailed, Name);
        IProxy snapshotServiceProxy = GetSnapshotServiceProxy();
        object[] array = new object[2] { this, null };
        VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Deleting snapshot tree rooted by '{0}' ('{1}')", InstanceId, Name));
        uint result = snapshotServiceProxy.InvokeMethod("DestroySnapshotTree", array);
        IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[1]);
        iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
        return iVMTask;
    }

    public void EndDeleteTree(IVMTask deleteTask)
    {
        EndMethod(deleteTask, VirtualizationOperation.DeleteSnapshotTree);
        VMTrace.TraceUserActionCompleted("Deleting snapshot tree completed successfully.");
    }

    public IVMTask BeginClearSnapshotState()
    {
        string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.SetStateFailed, Name);
        IProxy snapshotServiceProxy = GetSnapshotServiceProxy();
        object[] array = new object[2] { this, null };
        VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Clearing snapshot state '{0}' ('{1}')", InstanceId, Name));
        uint result = snapshotServiceProxy.InvokeMethod("ClearSnapshotState", array);
        IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[1]);
        iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
        return iVMTask;
    }

    public void EndClearSnapshotState(IVMTask clearTask)
    {
        EndMethod(clearTask, VirtualizationOperation.ClearSnapshotState);
        VMTrace.TraceUserActionCompleted("Clearing snapshot state completed successfully.");
    }

    protected override IVMTask BeginPutInternal(IDictionary<string, object> properties)
    {
        string text = base.Proxy.GetProperty("ElementName") as string;
        text = text ?? string.Empty;
        string embeddedInstance = GetEmbeddedInstance(properties);
        IProxy serviceProxy = GetServiceProxy();
        object[] array = new object[2] { embeddedInstance, null };
        string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.ModifyVirtualSystemSettingFailed, text);
        VMTrace.TraceUserActionInitiatedWithProperties(string.Format(CultureInfo.InvariantCulture, "Starting modify '{0}' ('{1}') on virtual system '{2}'.", Name, InstanceId, SystemName), properties);
        uint result = serviceProxy.InvokeMethod("ModifySystemSettings", array);
        IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[1]);
        iVMTask.PutProperties = properties;
        iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
        return iVMTask;
    }

    public byte[] GetThumbnailImage(int widthPixels, int heightPixels)
    {
        if (widthPixels <= 0)
        {
            throw new ArgumentOutOfRangeException("widthPixels");
        }
        if (heightPixels <= 0)
        {
            throw new ArgumentOutOfRangeException("heightPixels");
        }
        byte[] result = null;
        object[] array = new object[4] { this, widthPixels, heightPixels, null };
        VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Getting thumbnail image for virtual system '{0}' ('{1}')", InstanceId, Name), string.Format(CultureInfo.InvariantCulture, "WidthPixels = '{0}', heightPixels = '{1}'", widthPixels, heightPixels));
        if (GetServiceProxy().InvokeMethod("GetVirtualSystemThumbnailImage", array) == View.ErrorCodeSuccess)
        {
            result = array[3] as byte[];
            VMTrace.TraceUserActionCompleted("Getting thumbnail image completed successfully.");
        }
        return result;
    }

    public long GetSizeOfSystemFiles()
    {
        ulong num = 0uL;
        try
        {
            object[] array = new object[2] { this, null };
            VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Getting System files size  for virtual system '{0}' ('{1}')", InstanceId, Name));
            num = 0uL;
            if (GetServiceProxy().InvokeMethod("GetSizeOfSystemFiles", array) == View.ErrorCodeSuccess)
            {
                VMTrace.TraceUserActionCompleted("Getting size of System files completed successfully.");
                return (long)(ulong)array[1];
            }
            return 0L;
        }
        catch (Exception ex)
        {
            num = 0uL;
            VMTrace.TraceUserActionCompleted("Getting size of System files generated an exception {0}.", ex.ToString());
            return (long)num;
        }
    }

    private VirtualSystemType WmiVirtualSystemTypeToEnumVirtualSystemType(string wmiVirtualSystemType)
    {
        if (string.Equals(wmiVirtualSystemType, "Microsoft:Hyper-V:System:Realized", StringComparison.OrdinalIgnoreCase))
        {
            return VirtualSystemType.RealizedVirtualMachine;
        }
        if (string.Equals(wmiVirtualSystemType, "Microsoft:Hyper-V:System:Planned", StringComparison.OrdinalIgnoreCase))
        {
            return VirtualSystemType.PlannedVirtualMachine;
        }
        if (string.Equals(wmiVirtualSystemType, "Microsoft:Hyper-V:Snapshot:Realized", StringComparison.OrdinalIgnoreCase))
        {
            return VirtualSystemType.RealizedSnapshot;
        }
        if (string.Equals(wmiVirtualSystemType, "Microsoft:Hyper-V:Snapshot:Recovery", StringComparison.OrdinalIgnoreCase))
        {
            return VirtualSystemType.RecoverySnapshot;
        }
        if (string.Equals(wmiVirtualSystemType, "Microsoft:Hyper-V:Snapshot:Planned", StringComparison.OrdinalIgnoreCase))
        {
            return VirtualSystemType.PlannedSnapshot;
        }
        if (string.Equals(wmiVirtualSystemType, "Microsoft:Hyper-V:Snapshot:Missing", StringComparison.OrdinalIgnoreCase))
        {
            return VirtualSystemType.MissingSnapshot;
        }
        if (string.Equals(wmiVirtualSystemType, "Microsoft:Hyper-V:Snapshot:Replica:Standard", StringComparison.OrdinalIgnoreCase))
        {
            return VirtualSystemType.ReplicaSnapshot;
        }
        if (string.Equals(wmiVirtualSystemType, "Microsoft:Hyper-V:Snapshot:Replica:ApplicationConsistent", StringComparison.OrdinalIgnoreCase))
        {
            return VirtualSystemType.ApplicationConsistentReplicaSnapshot;
        }
        if (string.Equals(wmiVirtualSystemType, "Microsoft:Hyper-V:Snapshot:Replica:PlannedFailover", StringComparison.OrdinalIgnoreCase))
        {
            return VirtualSystemType.ReplicaSnapshotWithSyncedData;
        }
        return VirtualSystemType.ReplicaSettings;
    }

    public static VirtualSystemSubType WmiVirtualSystemSubTypeToEnumVirtualSystemSubType(string wmiVirtualSystemSubType)
    {
        if (string.IsNullOrEmpty(wmiVirtualSystemSubType))
        {
            return VirtualSystemSubType.Type1;
        }
        if (string.Equals(wmiVirtualSystemSubType, "Microsoft:Hyper-V:SubType:2", StringComparison.OrdinalIgnoreCase))
        {
            return VirtualSystemSubType.Type2;
        }
        if (string.Equals(wmiVirtualSystemSubType, "Microsoft:Hyper-V:SubType:1", StringComparison.OrdinalIgnoreCase))
        {
            return VirtualSystemSubType.Type1;
        }
        throw new ArgumentOutOfRangeException("wmiVirtualSystemSubType");
    }

    public static string EnumVirtualSystemSubTypeToWmiVirtualSystemSubType(VirtualSystemSubType subType)
    {
        return subType switch
        {
            VirtualSystemSubType.Type1 => "Microsoft:Hyper-V:SubType:1", 
            VirtualSystemSubType.Type2 => "Microsoft:Hyper-V:SubType:2", 
            _ => throw new ArgumentOutOfRangeException("subType"), 
        };
    }

    public override string ToString()
    {
        return Name;
    }

    public IEnumerable<IVMDeviceSetting> GetDeviceSettings()
    {
        return GetRelatedObjects<IVMDeviceSetting>(base.Associations.SystemSettingSettingData);
    }

    public IVMMemorySetting GetMemorySetting()
    {
        return GetRelatedObject<IVMMemorySetting>(base.Associations.SystemSettingMemorySettingData);
    }

    public IVMSecuritySetting GetSecuritySetting()
    {
        return GetRelatedObject<IVMSecuritySetting>(base.Associations.VirtualMachineSecuritySettingData);
    }

    public IVMStorageSetting GetStorageSetting()
    {
        return GetRelatedObject<IVMStorageSetting>(base.Associations.VirtualMachineStorageSettingData);
    }

    public IVMProcessorSetting GetProcessorSetting()
    {
        return GetRelatedObject<IVMProcessorSetting>(base.Associations.SystemSettingProcessorSettingData);
    }

    public IVMSyntheticDisplayControllerSetting GetSyntheticDisplayControllerSetting()
    {
        return GetRelatedObject<IVMSyntheticDisplayControllerSetting>(base.Associations.SystemSettingSyntheticDisplayControllerSettingData, throwIfNotFound: false);
    }

    public IVMSyntheticKeyboardControllerSetting GetSyntheticKeyboardControllerSetting()
    {
        IVMSyntheticKeyboardControllerSetting result = null;
        foreach (IVMDeviceSetting item in GetDeviceSettingsLimited(update: true, TimeSpan.Zero))
        {
            if (item.VMDeviceSettingType == VMDeviceSettingType.SynthKeyboard)
            {
                return (IVMSyntheticKeyboardControllerSetting)item;
            }
        }
        return result;
    }

    public IVMSyntheticMouseControllerSetting GetSyntheticMouseControllerSetting()
    {
        IVMSyntheticMouseControllerSetting result = null;
        foreach (IVMDeviceSetting item in GetDeviceSettingsLimited(update: true, TimeSpan.Zero))
        {
            if (item.VMDeviceSettingType == VMDeviceSettingType.SynthMouse)
            {
                return (IVMSyntheticMouseControllerSetting)item;
            }
        }
        return result;
    }

    public IEnumerable<IVMDeviceSetting> GetDeviceSettingsLimited(bool update, TimeSpan threshold)
    {
        if (update)
        {
            base.Proxy.UpdateOneCachedAssociation(base.Associations.SystemSettingSettingDataLimited, threshold);
        }
        return GetRelatedObjects<IVMDeviceSetting>(base.Associations.SystemSettingSettingDataLimited);
    }

    protected static bool IsServiceStartOperationValueValid(ServiceStartOperation operation)
    {
        if (operation != ServiceStartOperation.AlwaysStartup && operation != ServiceStartOperation.None)
        {
            return operation == ServiceStartOperation.RestartIfPreviouslyRunning;
        }
        return true;
    }

    protected static bool IsServiceStopOperationValueValid(ServiceStopOperation operation)
    {
        if (operation != ServiceStopOperation.PowerOff && operation != ServiceStopOperation.SaveState)
        {
            return operation == ServiceStopOperation.Shutdown;
        }
        return true;
    }
}

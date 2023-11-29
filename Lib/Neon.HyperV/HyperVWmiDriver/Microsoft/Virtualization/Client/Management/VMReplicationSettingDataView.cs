#define TRACE
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_ReplicationSettingData")]
internal class VMReplicationSettingDataView : View, IVMReplicationSettingData, IVirtualizationManagementObject, IPutableAsync, IPutable
{
    internal static class WmiMemberNames
    {
        public const string ApplicationConsistentSnapshotInterval = "ApplicationConsistentSnapshotInterval";

        public const string AuthenticationType = "AuthenticationType";

        public const string AutoResynchronizeEnabled = "AutoResynchronizeEnabled";

        public const string AutoResynchronizeIntervalEnd = "AutoResynchronizeIntervalEnd";

        public const string AutoResynchronizeIntervalStart = "AutoResynchronizeIntervalStart";

        public const string BypassProxyServer = "BypassProxyServer";

        public const string CertificateThumbPrint = "CertificateThumbPrint";

        public const string CompressionEnabled = "CompressionEnabled";

        public const string EnableWriteOrderPreservationAcrossDisks = "EnableWriteOrderPreservationAcrossDisks";

        public const string IncludedDisks = "IncludedDisks";

        public const string InstanceId = "InstanceID";

        public const string PrimaryConnectionPoint = "PrimaryConnectionPoint";

        public const string PrimaryHostSystem = "PrimaryHostSystem";

        public const string RecoveryConnectionPoint = "RecoveryConnectionPoint";

        public const string RecoveryHistory = "RecoveryHistory";

        public const string RecoveryHostSystem = "RecoveryHostSystem";

        public const string RecoveryServerPortNumber = "RecoveryServerPortNumber";

        public const string ReplicateHostKvpItems = "ReplicateHostKvpItems";

        public const string ReplicationInterval = "ReplicationInterval";

        public const string ReplicationProvider = "ReplicationProvider";

        public const string Put = "ModifyReplicationSettings";
    }

    public string InstanceId => GetProperty<string>("InstanceID");

    public FailoverReplicationAuthenticationType AuthenticationType
    {
        get
        {
            return (FailoverReplicationAuthenticationType)GetProperty<ushort>("AuthenticationType");
        }
        set
        {
            SetProperty("AuthenticationType", NumberConverter.Int32ToUInt16((int)value));
        }
    }

    public bool BypassProxyServer
    {
        get
        {
            return GetProperty<bool>("BypassProxyServer");
        }
        set
        {
            SetProperty("BypassProxyServer", value);
        }
    }

    public bool CompressionEnabled
    {
        get
        {
            return GetProperty<bool>("CompressionEnabled");
        }
        set
        {
            SetProperty("CompressionEnabled", value);
        }
    }

    public string RecoveryConnectionPoint
    {
        get
        {
            return GetProperty<string>("RecoveryConnectionPoint");
        }
        set
        {
            if (value == null)
            {
                value = string.Empty;
            }
            SetProperty("RecoveryConnectionPoint", value);
        }
    }

    public string RecoveryHostSystem => GetProperty<string>("RecoveryHostSystem");

    public string PrimaryConnectionPoint => GetProperty<string>("PrimaryConnectionPoint");

    public string PrimaryHostSystem => GetProperty<string>("PrimaryHostSystem");

    public int RecoveryServerPort
    {
        get
        {
            return NumberConverter.UInt16ToInt32(GetProperty<ushort>("RecoveryServerPortNumber"));
        }
        set
        {
            SetProperty("RecoveryServerPortNumber", NumberConverter.Int32ToUInt16(value));
        }
    }

    public string CertificateThumbPrint
    {
        get
        {
            return GetProperty<string>("CertificateThumbPrint");
        }
        set
        {
            if (value == null)
            {
                value = string.Empty;
            }
            SetProperty("CertificateThumbPrint", value);
        }
    }

    public int ApplicationConsistentSnapshotInterval
    {
        get
        {
            return NumberConverter.UInt16ToInt32(GetProperty<ushort>("ApplicationConsistentSnapshotInterval"));
        }
        set
        {
            SetProperty("ApplicationConsistentSnapshotInterval", NumberConverter.Int32ToUInt16(value));
        }
    }

    public FailoverReplicationInterval ReplicationInterval
    {
        get
        {
            return (FailoverReplicationInterval)NumberConverter.UInt16ToInt32(GetProperty<ushort>("ReplicationInterval"));
        }
        set
        {
            SetProperty("ReplicationInterval", NumberConverter.Int32ToUInt16(value switch
            {
                FailoverReplicationInterval.ThirtySeconds => 30, 
                FailoverReplicationInterval.FiveMinutes => 300, 
                FailoverReplicationInterval.FifteenMinutes => 900, 
                _ => 300, 
            }));
        }
    }

    public int RecoveryHistory
    {
        get
        {
            return NumberConverter.UInt16ToInt32(GetProperty<ushort>("RecoveryHistory"));
        }
        set
        {
            SetProperty("RecoveryHistory", NumberConverter.Int32ToUInt16(value));
        }
    }

    public bool AutoResynchronizeEnabled
    {
        get
        {
            return GetProperty<bool>("AutoResynchronizeEnabled");
        }
        set
        {
            SetProperty("AutoResynchronizeEnabled", value);
        }
    }

    public TimeSpan AutoResynchronizeIntervalStart
    {
        get
        {
            return GetProperty<TimeSpan>("AutoResynchronizeIntervalStart");
        }
        set
        {
            SetProperty("AutoResynchronizeIntervalStart", value);
        }
    }

    public TimeSpan AutoResynchronizeIntervalEnd
    {
        get
        {
            return GetProperty<TimeSpan>("AutoResynchronizeIntervalEnd");
        }
        set
        {
            SetProperty("AutoResynchronizeIntervalEnd", value);
        }
    }

    public bool EnableWriteOrderPreservationAcrossDisks
    {
        get
        {
            return GetProperty<bool>("EnableWriteOrderPreservationAcrossDisks");
        }
        set
        {
            SetProperty("EnableWriteOrderPreservationAcrossDisks", value);
        }
    }

    public WmiObjectPath[] IncludedDisks
    {
        get
        {
            return WmiObjectPath.FromStringArray(GetProperty<string[]>("IncludedDisks"));
        }
        set
        {
            SetProperty("IncludedDisks", WmiObjectPath.ToStringArray(value));
        }
    }

    public string ReplicationProvider
    {
        get
        {
            return GetProperty<string>("ReplicationProvider");
        }
        set
        {
            SetProperty("ReplicationProvider", value);
        }
    }

    public bool ReplicateHostKvpItems
    {
        get
        {
            return GetProperty<bool>("ReplicateHostKvpItems");
        }
        set
        {
            SetProperty("ReplicateHostKvpItems", value);
        }
    }

    public IVMComputerSystemBase VMComputerSystem => GetRelatedObject<IVMComputerSystemBase>(base.Associations.ElementSettingData);

    public IVMReplicationRelationship Relationship => GetRelatedObject<IVMReplicationRelationship>(base.Associations.SettingsDefineState);

    protected override IVMTask BeginPutInternal(IDictionary<string, object> properties)
    {
        string embeddedInstance = GetEmbeddedInstance(properties);
        IProxy failoverReplicationServiceProxy = GetFailoverReplicationServiceProxy();
        IVMComputerSystemBase vMComputerSystem = VMComputerSystem;
        object[] array = new object[4] { vMComputerSystem, embeddedInstance, null, null };
        VMTrace.TraceUserActionInitiatedWithProperties(string.Format(CultureInfo.InvariantCulture, "Starting modify virtual machine failover replication setting '{0}' ('{1}')", vMComputerSystem.InstanceId, vMComputerSystem.Name), properties);
        uint result = failoverReplicationServiceProxy.InvokeMethod("ModifyReplicationSettings", array);
        IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[2]);
        iVMTask.PutProperties = properties;
        return iVMTask;
    }
}

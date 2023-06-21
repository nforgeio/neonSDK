using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Microsoft.HyperV.PowerShell.Common;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Virtualization.Client.Management;

using ErrorMessages = Microsoft.HyperV.PowerShell.Common.ErrorMessages;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMReplication : VMReplicationBase, IUpdatable
{
    public ReplicationAuthenticationType AuthenticationType
    {
        get
        {
            return (ReplicationAuthenticationType)m_ReplicationSetting.GetData(UpdatePolicy.EnsureUpdated).AuthenticationType;
        }
        internal set
        {
            m_ReplicationSetting.GetData(UpdatePolicy.None).AuthenticationType = (FailoverReplicationAuthenticationType)value;
        }
    }

    public bool AutoResynchronizeEnabled
    {
        get
        {
            return m_ReplicationSetting.GetData(UpdatePolicy.EnsureUpdated).AutoResynchronizeEnabled;
        }
        internal set
        {
            m_ReplicationSetting.GetData(UpdatePolicy.None).AutoResynchronizeEnabled = value;
        }
    }

    public TimeSpan AutoResynchronizeIntervalEnd
    {
        get
        {
            return m_ReplicationSetting.GetData(UpdatePolicy.EnsureUpdated).AutoResynchronizeIntervalEnd;
        }
        internal set
        {
            m_ReplicationSetting.GetData(UpdatePolicy.None).AutoResynchronizeIntervalEnd = value;
        }
    }

    public TimeSpan AutoResynchronizeIntervalStart
    {
        get
        {
            return m_ReplicationSetting.GetData(UpdatePolicy.EnsureUpdated).AutoResynchronizeIntervalStart;
        }
        internal set
        {
            m_ReplicationSetting.GetData(UpdatePolicy.None).AutoResynchronizeIntervalStart = value;
        }
    }

    public bool BypassProxyServer
    {
        get
        {
            return m_ReplicationSetting.GetData(UpdatePolicy.EnsureUpdated).BypassProxyServer;
        }
        internal set
        {
            m_ReplicationSetting.GetData(UpdatePolicy.None).BypassProxyServer = value;
        }
    }

    public string CertificateThumbprint
    {
        get
        {
            return m_ReplicationSetting.GetData(UpdatePolicy.EnsureUpdated).CertificateThumbPrint;
        }
        internal set
        {
            m_ReplicationSetting.GetData(UpdatePolicy.None).CertificateThumbPrint = value;
        }
    }

    public bool CompressionEnabled
    {
        get
        {
            return m_ReplicationSetting.GetData(UpdatePolicy.EnsureUpdated).CompressionEnabled;
        }
        internal set
        {
            m_ReplicationSetting.GetData(UpdatePolicy.None).CompressionEnabled = value;
        }
    }

    public bool EnableWriteOrderPreservationAcrossDisks
    {
        get
        {
            return m_ReplicationSetting.GetData(UpdatePolicy.EnsureUpdated).EnableWriteOrderPreservationAcrossDisks;
        }
        internal set
        {
            m_ReplicationSetting.GetData(UpdatePolicy.None).EnableWriteOrderPreservationAcrossDisks = value;
        }
    }

    [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Usability is more important than the slight gain in efficiency here.")]
    public IList<HardDiskDrive> ExcludedDisks
    {
        get
        {
            WmiObjectPath[] includedDisks = m_ReplicationSetting.GetData(UpdatePolicy.EnsureUpdated).IncludedDisks;
            if (!includedDisks.IsNullOrEmpty())
            {
                IEnumerable<string> replicatedDisksIds = includedDisks.Select((WmiObjectPath drivePath) => ((IVirtualDiskSetting)ObjectLocator.GetVirtualizationManagementObject(base.Server, drivePath)).DeviceId);
                return (from disk in m_Vm.GetVirtualHardDiskDrives()
                    where !replicatedDisksIds.Contains(disk.GetVirtualDiskSetting(UpdatePolicy.None).DeviceId, StringComparer.OrdinalIgnoreCase)
                    select disk).ToList();
            }
            return new List<HardDiskDrive>();
        }
        internal set
        {
            IEnumerable<HardDiskDrive> virtualHardDiskDrives = m_Vm.GetVirtualHardDiskDrives();
            IEnumerable<string> passedInDiskIds = value.Select((HardDiskDrive excludedDisk) => excludedDisk.Id);
            HardDiskDrive[] array = virtualHardDiskDrives.Where((HardDiskDrive disk) => !passedInDiskIds.Contains(disk.Id, StringComparer.OrdinalIgnoreCase)).ToArray();
            if (array.Length == 0)
            {
                throw ExceptionHelper.CreateInvalidOperationException(string.Format(CultureInfo.CurrentCulture, ErrorMessages.VMReplication_NoReplicatedDisk, m_Vm.Name), null, this);
            }
            m_ReplicationSetting.GetData(UpdatePolicy.None).IncludedDisks = array.Select((HardDiskDrive disk) => disk.m_AttachedDiskSetting.GetData(UpdatePolicy.None).ManagementPath).ToArray();
        }
    }

    public DateTime? InitialReplicationStartTime
    {
        get
        {
            IVMTask replicationStartTask = m_Vm.GetReplicationStartTask();
            if (replicationStartTask != null && replicationStartTask.Status == VMTaskStatus.Running)
            {
                return replicationStartTask.ScheduledStartTime;
            }
            return null;
        }
    }

    public int RecoveryHistory
    {
        get
        {
            return m_ReplicationSetting.GetData(UpdatePolicy.EnsureUpdated).RecoveryHistory;
        }
        internal set
        {
            m_ReplicationSetting.GetData(UpdatePolicy.None).RecoveryHistory = value;
        }
    }

    public int ReplicaServerPort
    {
        get
        {
            return m_ReplicationSetting.GetData(UpdatePolicy.EnsureUpdated).RecoveryServerPort;
        }
        internal set
        {
            m_ReplicationSetting.GetData(UpdatePolicy.None).RecoveryServerPort = value;
        }
    }

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Kvp", Justification = "This is per spec.")]
    public bool ReplicateHostKvpItems
    {
        get
        {
            return m_ReplicationSetting.GetData(UpdatePolicy.EnsureUpdated).ReplicateHostKvpItems;
        }
        internal set
        {
            m_ReplicationSetting.GetData(UpdatePolicy.None).ReplicateHostKvpItems = value;
        }
    }

    public int ReplicationFrequencySec
    {
        get
        {
            return (int)m_ReplicationSetting.GetData(UpdatePolicy.EnsureUpdated).ReplicationInterval;
        }
        internal set
        {
            m_ReplicationSetting.GetData(UpdatePolicy.None).ReplicationInterval = (FailoverReplicationInterval)value;
        }
    }

    public DateTime? ResynchronizeStartTime
    {
        get
        {
            IVMTask replicationResyncTask = m_Vm.GetReplicationResyncTask();
            if (replicationResyncTask != null && replicationResyncTask.Status == VMTaskStatus.Running)
            {
                return replicationResyncTask.ScheduledStartTime;
            }
            return null;
        }
    }

    [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "VSS", Justification = "This is per spec.")]
    public int VSSSnapshotFrequencyHour
    {
        get
        {
            return m_ReplicationSetting.GetData(UpdatePolicy.EnsureUpdated).ApplicationConsistentSnapshotInterval;
        }
        internal set
        {
            m_ReplicationSetting.GetData(UpdatePolicy.None).ApplicationConsistentSnapshotInterval = value;
        }
    }

    [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "VSS", Justification = "This is per spec.")]
    public bool VSSSnapshotReplicationEnabled => VSSSnapshotFrequencyHour > 0;

    internal VMReplicationAuthorizationEntry AuthorizationEntry => new VMReplicationAuthorizationEntry(m_Vm.GetComputerSystemAs<IVMComputerSystem>(UpdatePolicy.EnsureAssociatorsUpdated).ReplicationAuthorizationSetting);

    internal VMReplicationType FailedOverReplicationType => (VMReplicationType)m_ReplicationRelationship.GetData(UpdatePolicy.EnsureUpdated).FailedOverReplicationType;

    internal bool IsEnabled => base.ReplicationState != VMReplicationState.Disabled;

    internal bool IsReplicatingToExternalProvider => !(ReplicationProvider ?? string.Empty).EndsWith("\"22391CDC-272C-4DDF-BA88-9BEFB1A0975C\"", StringComparison.OrdinalIgnoreCase);

    internal string ReplicationProvider => m_ReplicationSetting.GetData(UpdatePolicy.EnsureUpdated).ReplicationProvider;

    internal VirtualMachine VirtualMachine => m_Vm;

    private VMReplication(IVMReplicationSettingData replicationSetting, IVMReplicationRelationship replicationRelationship, VMReplicationRelationshipType relationshipType, VirtualMachine vm)
        : base(replicationSetting, replicationRelationship, relationshipType, vm)
    {
    }

    internal static VMReplication GetVMReplication(VirtualMachine vm, VMReplicationRelationshipType relationship)
    {
        IVMReplicationSettingData replicationSettingData = vm.GetComputerSystemAs<IVMComputerSystem>(UpdatePolicy.EnsureAssociatorsUpdated).GetReplicationSettingData((ReplicationRelationshipType)relationship, relationship == VMReplicationRelationshipType.Simple || vm.ReplicationMode == VMReplicationMode.Replica);
        if (replicationSettingData != null)
        {
            return new VMReplication(replicationSettingData, replicationSettingData.Relationship, relationship, vm);
        }
        return null;
    }

    internal static bool IsValidReplicationFrequency(int replicationFrequency)
    {
        return new int[3] { 30, 300, 900 }.Contains(replicationFrequency);
    }

    internal static void ReportInvalidModeError(string cmdletName, VMReplication replication)
    {
        string format = ErrorMessages.VMReplication_ActionNotApplicable_None;
        switch (replication.ReplicationMode)
        {
        case VMReplicationMode.Primary:
            format = ErrorMessages.VMReplication_ActionNotApplicableOnPrimary;
            break;
        case VMReplicationMode.Replica:
        case VMReplicationMode.ExtendedReplica:
            format = ErrorMessages.VMReplication_ActionNotApplicableOnReplica;
            break;
        case VMReplicationMode.TestReplica:
            format = ErrorMessages.VMReplication_ActionNotApplicableOnTestReplica;
            break;
        }
        throw ExceptionHelper.CreateOperationFailedException(string.Format(CultureInfo.CurrentCulture, format, cmdletName, replication.VMName));
    }

    internal static void ValidateReplicaServerName(VMReplication replication)
    {
        VMReplicationServer.TryGetClusterAndBroker(replication.Server, out var cluster, out var broker);
        string obj = cluster?.Name;
        string text = broker?.GetCapName();
        if (obj != null && (broker == null || text == null))
        {
            throw ExceptionHelper.CreateInvalidArgumentException(ErrorMessages.VMReplication_PrimaryBrokerNotEnabled);
        }
    }

    void IUpdatable.Put(IOperationWatcher watcher)
    {
        watcher.PerformPut(m_ReplicationSetting.GetData(UpdatePolicy.None), TaskDescriptions.SetVMReplicationSettings, null);
    }
}

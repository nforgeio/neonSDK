using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Vhd.PowerShell;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMReplicationHealth : VMReplicationBase
{
    private readonly string[] m_ReplicationHealthMessages;

    private readonly ReplicationStatistics m_ReplicationStatistics;

    private ulong m_InitialReplicationSize;

    private volatile bool m_InitialReplicationSizeNeedsUpdate = true;

    public IList<VMCurrentTask> CurrentTask
    {
        get
        {
            IEnumerable<IVMTask> replicationTasks = m_Vm.GetReplicationTasks();
            if (replicationTasks != null)
            {
                return (from task in replicationTasks
                    where task.Status == VMTaskStatus.Running
                    select task into runningTask
                    select new VMCurrentTask(runningTask)).ToList();
            }
            return new List<VMCurrentTask>();
        }
    }

    public DateTime? MonitoringStartTime
    {
        get
        {
            if (m_ReplicationStatistics != null)
            {
                return m_ReplicationStatistics.StartStatisticTime;
            }
            return null;
        }
    }

    public DateTime? MonitoringEndTime
    {
        get
        {
            if (m_ReplicationStatistics != null)
            {
                return m_ReplicationStatistics.EndStatisticTime;
            }
            return null;
        }
    }

    public VMReplicationType LastReplicationType => (VMReplicationType)m_ReplicationRelationship.GetData(UpdatePolicy.EnsureUpdated).LastReplicationType;

    public VMReplicationType FailedOverReplicationType => (VMReplicationType)m_ReplicationRelationship.GetData(UpdatePolicy.EnsureUpdated).FailedOverReplicationType;

    public DateTime? LastTestFailoverInitiatedTime
    {
        get
        {
            if (m_ReplicationStatistics != null && m_ReplicationStatistics.LastTestFailoverTime != Constants.Win32FileTimeEpoch)
            {
                return m_ReplicationStatistics.LastTestFailoverTime;
            }
            return null;
        }
    }

    [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "VSS", Justification = "This is per spec.")]
    public DateTime? LastVSSSnapshotTime
    {
        get
        {
            DateTime? lastApplicationConsistentReplicationTime = m_ReplicationRelationship.GetData(UpdatePolicy.EnsureUpdated).LastApplicationConsistentReplicationTime;
            if (lastApplicationConsistentReplicationTime != Constants.Win32FileTimeEpoch)
            {
                return lastApplicationConsistentReplicationTime;
            }
            return null;
        }
    }

    public ulong InitialReplicationSize => GetInitialReplicationSize();

    public ulong PendingReplicationSize
    {
        get
        {
            ulong num = 0uL;
            if (base.ReplicationMode == VMReplicationMode.Primary || (base.ReplicationMode == VMReplicationMode.Replica && base.ReplicationRelationshipType == VMReplicationRelationshipType.Extended))
            {
                num = GetInitialReplicationSize();
                if (num != 0L && !base.LastReplicationTime.HasValue)
                {
                    IVMTask replicationSendingInitialTask = m_Vm.GetReplicationSendingInitialTask();
                    if (replicationSendingInitialTask != null && !replicationSendingInitialTask.IsCompleted)
                    {
                        num = (ulong)((long)num * (long)(100 - replicationSendingInitialTask.PercentComplete)) / 100uL;
                    }
                }
                if (m_ReplicationStatistics != null)
                {
                    num += NumberConverter.Int64ToUInt64(m_ReplicationStatistics.PendingReplicationSize);
                }
            }
            return num;
        }
    }

    public ulong AverageReplicationSize
    {
        get
        {
            if (m_ReplicationStatistics != null && m_ReplicationStatistics.ReplicationSuccessCount > 0)
            {
                return NumberConverter.Int64ToUInt32(m_ReplicationStatistics.ReplicationSize / m_ReplicationStatistics.ReplicationSuccessCount);
            }
            return 0uL;
        }
    }

    public ulong MaximumReplicationSize
    {
        get
        {
            if (m_ReplicationStatistics != null)
            {
                return NumberConverter.Int64ToUInt64(m_ReplicationStatistics.MaxReplicationSize);
            }
            return 0uL;
        }
    }

    public TimeSpan? AverageReplicationLatency
    {
        get
        {
            if (m_ReplicationStatistics != null && m_ReplicationStatistics.ReplicationSuccessCount > 0)
            {
                return TimeSpan.FromSeconds(m_ReplicationStatistics.ReplicationLatency / m_ReplicationStatistics.ReplicationSuccessCount);
            }
            return null;
        }
    }

    public TimeSpan? MaximumReplicationLatency
    {
        get
        {
            if (m_ReplicationStatistics != null)
            {
                return TimeSpan.FromSeconds(m_ReplicationStatistics.MaxReplicationLatency);
            }
            return null;
        }
    }

    public uint ReplicationErrors
    {
        get
        {
            if (m_ReplicationStatistics != null)
            {
                return NumberConverter.Int32ToUInt32(m_ReplicationStatistics.ReplicationFailureCount) + NumberConverter.Int32ToUInt32(m_ReplicationStatistics.NetworkFailureCount) + NumberConverter.Int32ToUInt32(m_ReplicationStatistics.ApplicationConsistentSnapshotFailureCount);
            }
            return 0u;
        }
    }

    public uint SuccessfulReplicationCount
    {
        get
        {
            if (m_ReplicationStatistics != null)
            {
                return NumberConverter.Int32ToUInt32(m_ReplicationStatistics.ReplicationSuccessCount);
            }
            return 0u;
        }
    }

    public uint MissedReplicationCount
    {
        get
        {
            if (m_ReplicationStatistics != null)
            {
                return NumberConverter.Int32ToUInt32(m_ReplicationStatistics.ReplicationMissCount);
            }
            return 0u;
        }
    }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    public string[] ReplicationHealthDetails => m_ReplicationHealthMessages;

    internal VMReplicationHealth(VMReplication replication)
        : base(replication.GetReplicationSetting(UpdatePolicy.None), replication.GetReplicationRelationship(UpdatePolicy.None), replication.ReplicationRelationshipType, replication.VirtualMachine)
    {
        IVMComputerSystem computerSystemAs = m_Vm.GetComputerSystemAs<IVMComputerSystem>(UpdatePolicy.EnsureAssociatorsUpdated);
        computerSystemAs.CacheUpdated += delegate
        {
            m_InitialReplicationSizeNeedsUpdate = true;
        };
        ReplicationHealthInformation vMReplicationStatisticsEx = computerSystemAs.GetVMReplicationStatisticsEx(m_ReplicationRelationship.GetData(UpdatePolicy.None));
        m_ReplicationStatistics = vMReplicationStatisticsEx.ReplicationStatistics[0];
        m_ReplicationHealthMessages = vMReplicationStatisticsEx.HealthMessages.Select((MsvmError error) => error.Message).ToArray();
    }

    private ulong GetInitialReplicationSize()
    {
        if (!m_InitialReplicationSizeNeedsUpdate)
        {
            return m_InitialReplicationSize;
        }
        VMReplicationMode replicationMode = base.ReplicationMode;
        if ((replicationMode == VMReplicationMode.Primary || (replicationMode == VMReplicationMode.Replica && base.ReplicationRelationshipType == VMReplicationRelationshipType.Extended)) && !base.LastReplicationTime.HasValue)
        {
            m_InitialReplicationSize = base.ReplicatedDisks.Aggregate(0uL, (ulong current, HardDiskDrive disk) => current + GetSizeOfVirtualDisk(disk.Path));
            m_InitialReplicationSizeNeedsUpdate = false;
        }
        return m_InitialReplicationSize;
    }

    private ulong GetSizeOfVirtualDisk(string diskPath)
    {
        if (string.IsNullOrEmpty(diskPath))
        {
            return 0uL;
        }
        VirtualHardDisk virtualHardDisk = VhdUtilities.GetVirtualHardDisk(base.Server, diskPath);
        return virtualHardDisk.FileSize + GetSizeOfVirtualDisk(virtualHardDisk.ParentPath);
    }
}

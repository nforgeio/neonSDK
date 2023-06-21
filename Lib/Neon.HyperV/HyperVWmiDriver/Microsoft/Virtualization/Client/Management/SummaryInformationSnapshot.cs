using System;
using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal class SummaryInformationSnapshot : ISummaryInformationSnapshot, ISummaryInformationProperties, ISummaryInformationPropertiesBase
{
    private readonly string m_Name;

    private readonly string m_ElementName;

    private readonly string m_HostComputerSystemName;

    private readonly VMComputerSystemState m_State;

    private readonly bool m_RdpEnhancedModeAvailable;

    private readonly VirtualSystemSubType m_VirtualSystemSubType;

    private readonly int m_ProcessorLoad;

    private readonly TimeSpan m_Uptime;

    private readonly VMComputerSystemHealthState m_HealthState;

    private readonly long m_AssignedMemory;

    private readonly int m_MemoryAvailable;

    private readonly long m_MemoryDemand;

    private readonly int m_AvailableMemoryBuffer;

    private readonly bool m_SwapFilesInUse;

    private readonly bool m_MemorySpansPhysicalNumaNodes;

    private readonly DateTime m_CreationTime;

    private readonly VMHeartbeatStatus m_Heartbeat;

    private readonly string m_Notes;

    private readonly string m_Version;

    private readonly bool m_Shielded;

    private readonly FailoverReplicationMode m_ReplicationMode;

    private readonly WmiObjectPath m_TestReplicaSystemPath;

    private readonly int m_ThumbnailImageWidth;

    private readonly int m_ThumbnailImageHeight;

    private readonly VMComputerSystemOperationalStatus[] m_OperationalStatus;

    private readonly string[] m_StatusDescriptions;

    private readonly byte[] m_ThumbnailImage;

    private readonly FailoverReplicationState[] m_ReplicationStateEx;

    private readonly FailoverReplicationHealth[] m_ReplicationHealthEx;

    private readonly bool[] m_ReplicatingToDefaultProvider;

    public string Name => m_Name;

    public string ElementName => m_ElementName;

    public string HostComputerSystemName => m_HostComputerSystemName;

    public VMComputerSystemState State => m_State;

    public bool RdpEnhancedModeAvailable => m_RdpEnhancedModeAvailable;

    public VirtualSystemSubType VirtualSystemSubType => m_VirtualSystemSubType;

    public int ProcessorLoad => m_ProcessorLoad;

    public TimeSpan Uptime => m_Uptime;

    public VMComputerSystemHealthState HealthState => m_HealthState;

    public long AssignedMemory => m_AssignedMemory;

    public int MemoryAvailable => m_MemoryAvailable;

    public long MemoryDemand => m_MemoryDemand;

    public int AvailableMemoryBuffer => m_AvailableMemoryBuffer;

    public bool SwapFilesInUse => m_SwapFilesInUse;

    public bool MemorySpansPhysicalNumaNodes => m_MemorySpansPhysicalNumaNodes;

    public DateTime CreationTime => m_CreationTime;

    public VMHeartbeatStatus Heartbeat => m_Heartbeat;

    public string Notes => m_Notes;

    public string Version => m_Version;

    public bool Shielded => m_Shielded;

    public FailoverReplicationMode ReplicationMode => m_ReplicationMode;

    public WmiObjectPath TestReplicaSystemPath => m_TestReplicaSystemPath;

    public int ThumbnailImageWidth => m_ThumbnailImageWidth;

    public int ThumbnailImageHeight => m_ThumbnailImageHeight;

    internal SummaryInformationSnapshot(ISummaryInformationProperties original)
    {
        m_Name = original.Name;
        m_ElementName = original.ElementName;
        m_HostComputerSystemName = original.HostComputerSystemName;
        m_State = original.State;
        m_RdpEnhancedModeAvailable = original.RdpEnhancedModeAvailable;
        m_VirtualSystemSubType = original.VirtualSystemSubType;
        m_ProcessorLoad = original.ProcessorLoad;
        m_Uptime = original.Uptime;
        m_HealthState = original.HealthState;
        m_AssignedMemory = original.AssignedMemory;
        m_MemoryAvailable = original.MemoryAvailable;
        m_MemoryDemand = original.MemoryDemand;
        m_AvailableMemoryBuffer = original.AvailableMemoryBuffer;
        m_SwapFilesInUse = original.SwapFilesInUse;
        m_MemorySpansPhysicalNumaNodes = original.MemorySpansPhysicalNumaNodes;
        m_CreationTime = original.CreationTime;
        m_Heartbeat = original.Heartbeat;
        m_Notes = original.Notes;
        m_Version = original.Version;
        m_Shielded = original.Shielded;
        m_ReplicationMode = original.ReplicationMode;
        m_TestReplicaSystemPath = original.TestReplicaSystemPath;
        m_ThumbnailImageWidth = original.ThumbnailImageWidth;
        m_ThumbnailImageHeight = original.ThumbnailImageHeight;
        m_OperationalStatus = original.GetOperationalStatus();
        m_StatusDescriptions = original.GetStatusDescriptions();
        m_ThumbnailImage = original.GetThumbnailImage();
        m_ReplicationStateEx = original.GetReplicationStateEx();
        m_ReplicationHealthEx = original.GetReplicationHealthEx();
        m_ReplicatingToDefaultProvider = original.GetReplicatingToDefaultProvider();
    }

    internal SummaryInformationSnapshot(ICimInstance cimInstance)
    {
        CimKeyedCollection<CimProperty> cimInstanceProperties = cimInstance.CimInstanceProperties;
        m_Name = (string)cimInstanceProperties["Name"].Value;
        m_ElementName = (string)cimInstanceProperties["ElementName"].Value;
        object value = cimInstanceProperties["CreationTime"].Value;
        if (value != null)
        {
            m_CreationTime = (DateTime)value;
        }
        m_Notes = (string)cimInstanceProperties["Notes"].Value;
    }

    public VMComputerSystemOperationalStatus[] GetOperationalStatus()
    {
        return m_OperationalStatus;
    }

    public string[] GetStatusDescriptions()
    {
        return m_StatusDescriptions;
    }

    public byte[] GetThumbnailImage()
    {
        return m_ThumbnailImage;
    }

    public FailoverReplicationState[] GetReplicationStateEx()
    {
        return m_ReplicationStateEx;
    }

    public FailoverReplicationHealth[] GetReplicationHealthEx()
    {
        return m_ReplicationHealthEx;
    }

    public bool[] GetReplicatingToDefaultProvider()
    {
        return m_ReplicatingToDefaultProvider;
    }
}

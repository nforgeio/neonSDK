using System;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class ISummaryInformationContract : ISummaryInformation, ISummaryInformationProperties, ISummaryInformationPropertiesBase, ISummaryInformationBase, IVirtualizationManagementObject
{
	public abstract string Name { get; }

	public abstract string ElementName { get; }

	public abstract string HostComputerSystemName { get; }

	public abstract VMComputerSystemState State { get; }

	public abstract bool RdpEnhancedModeAvailable { get; }

	public abstract VirtualSystemSubType VirtualSystemSubType { get; }

	public abstract int ProcessorLoad { get; }

	public abstract TimeSpan Uptime { get; }

	public abstract VMComputerSystemHealthState HealthState { get; }

	public abstract long AssignedMemory { get; }

	public abstract int MemoryAvailable { get; }

	public abstract long MemoryDemand { get; }

	public abstract int AvailableMemoryBuffer { get; }

	public abstract bool SwapFilesInUse { get; }

	public abstract bool MemorySpansPhysicalNumaNodes { get; }

	public abstract DateTime CreationTime { get; }

	public abstract VMHeartbeatStatus Heartbeat { get; }

	public abstract string Notes { get; }

	public abstract string Version { get; }

	public abstract bool Shielded { get; }

	public abstract FailoverReplicationMode ReplicationMode { get; }

	public abstract WmiObjectPath TestReplicaSystemPath { get; }

	public abstract int ThumbnailImageWidth { get; }

	public abstract int ThumbnailImageHeight { get; }

	public abstract Server Server { get; }

	public abstract WmiObjectPath ManagementPath { get; }

	public event EventHandler Deleted;

	public event EventHandler CacheUpdated;

	public ISummaryInformationSnapshot CreateSnapshot()
	{
		return null;
	}

	public void UpdatePropertyCache(SummaryInformationRequest requestedInformation)
	{
	}

	public void UpdatePropertyCache(TimeSpan threshold, SummaryInformationRequest requestedInformation)
	{
	}

	public abstract VMComputerSystemOperationalStatus[] GetOperationalStatus();

	public abstract string[] GetStatusDescriptions();

	public abstract byte[] GetThumbnailImage();

	public abstract FailoverReplicationState[] GetReplicationStateEx();

	public abstract FailoverReplicationHealth[] GetReplicationHealthEx();

	public abstract bool[] GetReplicatingToDefaultProvider();

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

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Virtualization.Client.Management;

internal interface ISummaryInformationProperties : ISummaryInformationPropertiesBase
{
	int ProcessorLoad { get; }

	long AssignedMemory { get; }

	int MemoryAvailable { get; }

	long MemoryDemand { get; }

	int AvailableMemoryBuffer { get; }

	bool SwapFilesInUse { get; }

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
	bool MemorySpansPhysicalNumaNodes { get; }

	VMHeartbeatStatus Heartbeat { get; }

	FailoverReplicationMode ReplicationMode { get; }

	WmiObjectPath TestReplicaSystemPath { get; }

	int ThumbnailImageWidth { get; }

	int ThumbnailImageHeight { get; }

	byte[] GetThumbnailImage();

	FailoverReplicationState[] GetReplicationStateEx();

	FailoverReplicationHealth[] GetReplicationHealthEx();

	bool[] GetReplicatingToDefaultProvider();
}

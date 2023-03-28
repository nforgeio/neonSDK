using System;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_ReplicationStatistics")]
internal class ReplicationStatistics : EmbeddedInstance
{
	internal static class WmiPropertyNames
	{
		public const string StartStatisticTime = "StartStatisticTime";

		public const string EndStatisticTime = "EndStatisticTime";

		public const string ReplicationSuccessCount = "ReplicationSuccessCount";

		public const string ReplicationSize = "ReplicationSize";

		public const string MaxReplicationSize = "MaxReplicationSize";

		public const string ReplicationMissCount = "ReplicationMissCount";

		public const string MaxReplicationLatency = "MaxReplicationLatency";

		public const string NetworkFailureCount = "NetworkFailureCount";

		public const string ReplicationFailureCount = "ReplicationFailureCount";

		public const string PendingReplicationSize = "PendingReplicationSize";

		public const string ApplicationConsistentSnapshotFailureCount = "ApplicationConsistentSnapshotFailureCount";

		public const string ReplicationLatency = "ReplicationLatency";

		public const string LastTestFailoverTime = "LastTestFailoverTime";

		public const string ReplicationHealth = "ReplicationHealth";
	}

	private static readonly DateTime gm_MinFileTime = DateTime.FromFileTime(0L);

	public DateTime StartStatisticTime => GetProperty<DateTime>("StartStatisticTime");

	public DateTime EndStatisticTime => GetProperty<DateTime>("EndStatisticTime");

	public int ReplicationSuccessCount => NumberConverter.UInt32ToInt32(GetProperty("ReplicationSuccessCount", 0u));

	public long ReplicationSize => NumberConverter.UInt64ToInt64(GetProperty("ReplicationSize", 0uL));

	public long MaxReplicationSize => NumberConverter.UInt64ToInt64(GetProperty("MaxReplicationSize", 0uL));

	public int ReplicationMissCount => NumberConverter.UInt32ToInt32(GetProperty("ReplicationMissCount", 0u));

	public int MaxReplicationLatency => NumberConverter.UInt32ToInt32(GetProperty("MaxReplicationLatency", 0u));

	public int NetworkFailureCount => NumberConverter.UInt32ToInt32(GetProperty("NetworkFailureCount", 0u));

	public int ReplicationFailureCount => NumberConverter.UInt32ToInt32(GetProperty("ReplicationFailureCount", 0u));

	public long PendingReplicationSize => NumberConverter.UInt64ToInt64(GetProperty("PendingReplicationSize", 0uL));

	public int ApplicationConsistentSnapshotFailureCount => NumberConverter.UInt32ToInt32(GetProperty("ApplicationConsistentSnapshotFailureCount", 0u));

	public int ReplicationLatency => NumberConverter.UInt32ToInt32(GetProperty("ReplicationLatency", 0u));

	public DateTime? LastTestFailoverTime
	{
		get
		{
			DateTime? property = GetProperty<DateTime?>("LastTestFailoverTime");
			if (property.HasValue && property.Value == gm_MinFileTime)
			{
				return null;
			}
			return property;
		}
	}

	public FailoverReplicationHealth ReplicationHealth => (FailoverReplicationHealth)GetProperty("ReplicationHealth", 0u);
}

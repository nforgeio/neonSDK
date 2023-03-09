using System;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_ReplicationRelationship")]
internal class VMReplicationRelationshipView : View, IVMReplicationRelationship, IVirtualizationManagementObject
{
	internal static class WmiMemberNames
	{
		public const string FailedOverReplicationType = "FailedOverReplicationType";

		public const string InstanceId = "InstanceID";

		public const string LastApplicationConsistentReplicationTime = "LastApplicationConsistentReplicationTime";

		public const string LastApplyTime = "LastApplyTime";

		public const string LastReplicationTime = "LastReplicationTime";

		public const string LastReplicationType = "LastReplicationType";

		public const string ReplicationHealth = "ReplicationHealth";

		public const string ReplicationState = "ReplicationState";
	}

	private static readonly DateTime gm_MinFileTime = DateTime.FromFileTime(0L);

	public string InstanceId => GetProperty<string>("InstanceID");

	public FailoverReplicationState ReplicationState => (FailoverReplicationState)GetProperty<ushort>("ReplicationState");

	public FailoverReplicationHealth ReplicationHealth => (FailoverReplicationHealth)GetProperty<ushort>("ReplicationHealth");

	public DateTime? LastApplicationConsistentReplicationTime => GetWmiNormalizedDate("LastApplicationConsistentReplicationTime");

	public DateTime? LastApplyTime => GetWmiNormalizedDate("LastApplyTime");

	public DateTime? LastReplicationTime => GetWmiNormalizedDate("LastReplicationTime");

	public ReplicationType LastReplicationType => (ReplicationType)GetProperty<ushort>("LastReplicationType");

	public ReplicationType FailedOverReplicationType => (ReplicationType)GetProperty<ushort>("FailedOverReplicationType");

	private DateTime? GetWmiNormalizedDate(string propertyName)
	{
		DateTime property = GetProperty<DateTime>(propertyName);
		if (property == gm_MinFileTime)
		{
			return null;
		}
		return property;
	}
}

using System;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_ReplicationRelationship")]
internal interface IVMReplicationRelationship : IVirtualizationManagementObject
{
	[Key]
	string InstanceId { get; }

	FailoverReplicationState ReplicationState { get; }

	FailoverReplicationHealth ReplicationHealth { get; }

	DateTime? LastApplicationConsistentReplicationTime { get; }

	DateTime? LastApplyTime { get; }

	DateTime? LastReplicationTime { get; }

	ReplicationType LastReplicationType { get; }

	ReplicationType FailedOverReplicationType { get; }
}

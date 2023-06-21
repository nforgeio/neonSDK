using System;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IVMReplicationRelationshipContract : IVMReplicationRelationship, IVirtualizationManagementObject
{
    public string InstanceId => null;

    public FailoverReplicationState ReplicationState => FailoverReplicationState.Disabled;

    public FailoverReplicationHealth ReplicationHealth => FailoverReplicationHealth.NotApplicable;

    public DateTime? LastApplicationConsistentReplicationTime => null;

    public DateTime? LastApplyTime => null;

    public DateTime? LastReplicationTime => null;

    public ReplicationType LastReplicationType => ReplicationType.NotApplicable;

    public ReplicationType FailedOverReplicationType => ReplicationType.NotApplicable;

    public abstract Server Server { get; }

    public abstract WmiObjectPath ManagementPath { get; }

    public event EventHandler Deleted;

    public event EventHandler CacheUpdated;

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

using System;
using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IReplicationServiceContract : IReplicationService, IVirtualizationManagementObject
{
    public ushort[] OperationalStatus => null;

    public string[] StatusDescriptions => null;

    public IFailoverReplicationServiceSetting Setting => null;

    public IEnumerable<IFailoverReplicationAuthorizationSetting> AuthorizationSettings => null;

    public abstract Server Server { get; }

    public abstract WmiObjectPath ManagementPath { get; }

    public event EventHandler Deleted;

    public event EventHandler CacheUpdated;

    public IVMTask BeginStartReplication(IVMComputerSystem computerSystem, InitialReplicationType initialReplicationType, string initialReplicationShare, DateTime scheduledDateTime)
    {
        return null;
    }

    public void EndStartReplication(IVMTask task)
    {
    }

    public IVMTask BeginImportInitialReplica(IVMComputerSystem computerSystem, string initialReplicationImportLocation)
    {
        return null;
    }

    public void EndImportInitialReplica(IVMTask task)
    {
    }

    public IVMTask BeginRemoveReplicationRelationshipEx(IVMComputerSystem computerSystem, IVMReplicationRelationship replicationRelationship)
    {
        return null;
    }

    public void EndRemoveReplicationRelationshipEx(IVMTask task)
    {
    }

    public IVMTask BeginCreateReplicationRelationship(IVMComputerSystem computerSystem, IVMReplicationSettingData replicationSettingData)
    {
        return null;
    }

    public void EndCreateReplicationRelationship(IVMTask task)
    {
    }

    public IVMTask BeginReverseReplicationRelationship(IVMComputerSystem computerSystem, IVMReplicationSettingData replicationSettingData)
    {
        return null;
    }

    public void EndReverseReplicationRelationship(IVMTask task)
    {
    }

    public IVMTask BeginChangeReplicationModeToPrimary(IVMComputerSystem computerSystem, IVMReplicationRelationship replicationRelationship)
    {
        return null;
    }

    public void EndChangeReplicationModeToPrimary(IVMTask task)
    {
    }

    public IVMTask BeginResynchronizeReplication(IVMComputerSystem computerSystem, DateTime scheduledDateTime)
    {
        return null;
    }

    public void EndResynchronizeReplication(IVMTask task)
    {
    }

    public IVMTask BeginResetReplicationStatisticsEx(IVMComputerSystem computerSystem, IVMReplicationRelationship replicationRelationship)
    {
        return null;
    }

    public void ResetReplicationStatisticsEx(IVMComputerSystem computerSystem, IVMReplicationRelationship replicationRelationship)
    {
    }

    public void EndResetReplicationStatisticsEx(IVMTask task)
    {
    }

    public string[] GetSystemCertificates()
    {
        return null;
    }

    public IVMTask BeginInitiateFailover(IVMComputerSystem computerSystem, IVMComputerSystemSetting snapshot)
    {
        return null;
    }

    public void EndInitiateFailover(IVMTask task)
    {
    }

    public IVMTask BeginRevertFailover(IVMComputerSystem computerSystem)
    {
        return null;
    }

    public void EndRevertFailover(IVMTask task)
    {
    }

    public IVMTask BeginCommitFailover(IVMComputerSystem computerSystem)
    {
        return null;
    }

    public void EndCommitFailover(IVMTask task)
    {
    }

    public IVMTask BeginCreateTestVirtualSystem(IVMComputerSystem computerSystem, IVMComputerSystemSetting snapshot)
    {
        return null;
    }

    public IVMComputerSystem EndCreateTestVirtualSystem(IVMTask task, string instanceId)
    {
        return null;
    }

    public IVMTask BeginTestReplicationConnection(string recoveryConnectionPoint, ushort recoveryServerPortNumber, RecoveryAuthenticationType authenticationType, string certificateThumbPrint, bool bypassProxyServer)
    {
        return null;
    }

    public void EndTestReplicationConnection(IVMTask task)
    {
    }

    public IVMTask BeginAddAuthorizationEntry(ReplicationAuthorizationEntry replicationAuthEntry)
    {
        return null;
    }

    public void AddAuthorizationEntry(ReplicationAuthorizationEntry replicationAuthEntry)
    {
    }

    public void EndAddAuthorizationEntry(IVMTask task)
    {
    }

    public void ModifyAuthorizationEntry(ReplicationAuthorizationEntry replicationAuthEntry)
    {
    }

    public IVMTask BeginSetAuthorizationEntry(IVMComputerSystem computerSystem, ReplicationAuthorizationEntry replicationAuthEntry)
    {
        return null;
    }

    public void SetAuthorizationEntry(IVMComputerSystem computerSystem, ReplicationAuthorizationEntry replicationAuthEntry)
    {
    }

    public void EndSetAuthorizationEntry(IVMTask task)
    {
    }

    public void RemoveAuthorizationEntry(string allowedPrimaryHostSystem)
    {
    }

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

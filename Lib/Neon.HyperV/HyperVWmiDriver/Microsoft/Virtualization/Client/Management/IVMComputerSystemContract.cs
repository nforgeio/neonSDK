using System;
using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IVMComputerSystemContract : IVMComputerSystem, IVMComputerSystemBase, IVirtualizationManagementObject, IDeleteableAsync, IDeleteable, IMetricMeasurableElement
{
    public FailoverReplicationMode ReplicationMode => FailoverReplicationMode.None;

    public EnhancedSessionModeStateType EnhancedSessionModeState => (EnhancedSessionModeStateType)0;

    public IEnumerable<IVMCollection> CollectingCollections => null;

    public IFailoverReplicationAuthorizationSetting ReplicationAuthorizationSetting => null;

    public IVMComputerSystem TestReplicaSystem => null;

    public abstract string Name { get; }

    public abstract DateTime TimeOfLastStateChange { get; }

    public abstract string InstanceId { get; }

    public abstract VMComputerSystemState State { get; }

    public abstract VMComputerSystemHealthState HealthState { get; }

    public abstract int NumberOfNumaNodes { get; }

    public abstract int? HwThreadsPerCore { get; }

    public abstract DateTime TimeOfLastConfigurationChange { get; }

    public abstract IEnumerable<IVMMemory> Memory { get; }

    public abstract IVMSecurityInformation SecurityInformation { get; }

    public abstract IVMKeyboard Keyboard { get; }

    public abstract IVMShutdownComponent ShutdownComponent { get; }

    public abstract IVMVssComponent VssComponent { get; }

    public abstract IVMComputerSystemSetting Setting { get; }

    public abstract IVMExportSetting ExportSetting { get; }

    public abstract IEnumerable<IVMComputerSystemSetting> Snapshots { get; }

    public abstract IEnumerable<IVMComputerSystemSetting> ReplicaSnapshots { get; }

    public abstract IEnumerable<IVMTask> Tasks { get; }

    public abstract int NumberOfSnapshots { get; }

    public abstract IHostComputerSystem HostSystem { get; }

    public abstract IVMReplicationSettingData VMReplicationSettingData { get; }

    public abstract IVMReplicationSettingData VMExtendedReplicationSettingData { get; }

    public abstract IVMReplicationRelationship VMReplicationRelationship { get; }

    public abstract IVMReplicationRelationship VMExtendedReplicationRelationship { get; }

    public abstract Server Server { get; }

    public abstract WmiObjectPath ManagementPath { get; }

    public abstract MetricEnabledState AggregateMetricEnabledState { get; }

    public event SnapshotCreatedEventHandler SnapshotCreated;

    public event EventHandler Deleted;

    public event EventHandler CacheUpdated;

    public IVMTask BeginSetReplicationStateEx(IVMReplicationRelationship replicationRelationship, FailoverReplicationState state)
    {
        return null;
    }

    public void EndSetReplicationState(IVMTask task)
    {
    }

    public IVMTask BeginTakeSnapshot()
    {
        return null;
    }

    public IVMTask BeginTakeSnapshot(bool takeAutomaticSnapshot)
    {
        return null;
    }

    public IVMComputerSystemSetting EndTakeSnapshot(IVMTask snapshotTask)
    {
        return null;
    }

    public IVMTask BeginInjectNonMaskableInterrupt()
    {
        return null;
    }

    public void EndInjectNonMaskableInterrupt(IVMTask injectNonMaskableInterruptTask)
    {
    }

    public IVMTask BeginUpgrade()
    {
        return null;
    }

    public void EndUpgrade(IVMTask upgradeTask)
    {
    }

    public ISummaryInformation GetVMSummaryInformation(SummaryInformationRequest requestedInformation)
    {
        return null;
    }

    public ReplicationHealthInformation GetVMReplicationStatisticsEx(IVMReplicationRelationship replicationRelationship)
    {
        return null;
    }

    public void RemoveKvpItem(string name, KvpItemPool pool)
    {
    }

    public bool IsExtendedReplicationEnabled()
    {
        return false;
    }

    public bool IsSnapshotAvailable()
    {
        return false;
    }

    public bool IsProductionSnapshotAvailable()
    {
        return false;
    }

    public bool WasOnlineProductionSnapshot()
    {
        return false;
    }

    public bool IsUpgradable()
    {
        return false;
    }

    public IEnumerable<ITerminalConnection> GetTerminalConnections()
    {
        return null;
    }

    public bool DoesTerminalConnectionExist()
    {
        return false;
    }

    public IEnumerable<IVMMigrationTask> GetMigrationTasks()
    {
        return null;
    }

    public abstract VMComputerSystemOperationalStatus[] GetOperationalStatus();

    public abstract string[] GetStatusDescriptions();

    public abstract IVMTask BeginAddDevice(IVMDeviceSetting deviceToAdd);

    public abstract IVMDeviceSetting EndAddDevice(IVMTask addDeviceTask);

    public abstract IVMTask BeginSetState(VMComputerSystemState state);

    public abstract void EndSetState(IVMTask setStateTask);

    public abstract IVMComputerSystemSetting GetPreviousSnapshot(bool needsRefresh);

    public abstract IVMReplicationSettingData GetReplicationSettingData(ReplicationRelationshipType relationshipType, bool throwIfNotFound);

    public abstract IVMReplicationRelationship GetReplicationRelationship(ReplicationRelationshipType relationshipType, bool throwIfNotFound);

    public abstract void RemoveFromCache();

    public abstract IVMTask BeginDelete();

    public abstract void EndDelete(IVMTask deleteTask);

    public abstract void Delete();

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

    public abstract IReadOnlyCollection<IMetricValue> GetMetricValues();
}

using System;
using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IVMPlannedComputerSystemContract : IVMPlannedComputerSystem, IVMComputerSystemBase, IVirtualizationManagementObject, IDeleteableAsync, IDeleteable
{
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

    public event EventHandler Deleted;

    public event EventHandler CacheUpdated;

    public IVMTask BeginRemoveFromGroupById(Guid collectionId)
    {
        return null;
    }

    public void EndRemoveFromGroupById(IVMTask task)
    {
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
}

using System;
using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("CIM_ComputerSystem")]
internal interface IVMComputerSystemBase : IVirtualizationManagementObject, IDeleteableAsync, IDeleteable
{
    string Name { get; }

    DateTime TimeOfLastStateChange { get; }

    [Key]
    string InstanceId { get; }

    VMComputerSystemState State { get; }

    VMComputerSystemHealthState HealthState { get; }

    int NumberOfNumaNodes { get; }

    int? HwThreadsPerCore { get; }

    DateTime TimeOfLastConfigurationChange { get; }

    IEnumerable<IVMMemory> Memory { get; }

    IVMSecurityInformation SecurityInformation { get; }

    IVMKeyboard Keyboard { get; }

    IVMShutdownComponent ShutdownComponent { get; }

    IVMVssComponent VssComponent { get; }

    IVMComputerSystemSetting Setting { get; }

    IVMExportSetting ExportSetting { get; }

    IEnumerable<IVMComputerSystemSetting> Snapshots { get; }

    IEnumerable<IVMComputerSystemSetting> ReplicaSnapshots { get; }

    IEnumerable<IVMTask> Tasks { get; }

    int NumberOfSnapshots { get; }

    IHostComputerSystem HostSystem { get; }

    IVMReplicationSettingData VMReplicationSettingData { get; }

    IVMReplicationSettingData VMExtendedReplicationSettingData { get; }

    IVMReplicationRelationship VMReplicationRelationship { get; }

    IVMReplicationRelationship VMExtendedReplicationRelationship { get; }

    VMComputerSystemOperationalStatus[] GetOperationalStatus();

    string[] GetStatusDescriptions();

    IVMTask BeginAddDevice(IVMDeviceSetting deviceToAdd);

    IVMDeviceSetting EndAddDevice(IVMTask addDeviceTask);

    IVMTask BeginSetState(VMComputerSystemState state);

    void EndSetState(IVMTask setStateTask);

    IVMComputerSystemSetting GetPreviousSnapshot(bool needsRefresh);

    IVMReplicationSettingData GetReplicationSettingData(ReplicationRelationshipType relationshipType, bool throwIfNotFound);

    IVMReplicationRelationship GetReplicationRelationship(ReplicationRelationshipType relationshipType, bool throwIfNotFound);

    void RemoveFromCache();
}

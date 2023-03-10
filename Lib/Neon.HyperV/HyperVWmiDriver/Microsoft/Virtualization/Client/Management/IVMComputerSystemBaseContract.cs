using System;
using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IVMComputerSystemBaseContract : IVMComputerSystemBase, IVirtualizationManagementObject, IDeleteableAsync, IDeleteable
{
	public string Name => null;

	public DateTime TimeOfLastStateChange => default(DateTime);

	public string InstanceId => null;

	public VMComputerSystemState State => VMComputerSystemState.Unknown;

	public VMComputerSystemHealthState HealthState => VMComputerSystemHealthState.Unknown;

	public int NumberOfNumaNodes => 0;

	public int? HwThreadsPerCore => null;

	public DateTime TimeOfLastConfigurationChange => default(DateTime);

	public IEnumerable<IVMMemory> Memory => null;

	public IVMSecurityInformation SecurityInformation => null;

	public IVMKeyboard Keyboard => null;

	public IVMShutdownComponent ShutdownComponent => null;

	public IVMVssComponent VssComponent => null;

	public IVMComputerSystemSetting Setting => null;

	public IVMExportSetting ExportSetting => null;

	public IEnumerable<IVMComputerSystemSetting> Snapshots => null;

	public IEnumerable<IVMComputerSystemSetting> ReplicaSnapshots => null;

	public IEnumerable<IVMTask> Tasks => null;

	public int NumberOfSnapshots => 0;

	public IHostComputerSystem HostSystem => null;

	public IVMReplicationSettingData VMReplicationSettingData => null;

	public IVMReplicationSettingData VMExtendedReplicationSettingData => null;

	public IVMReplicationRelationship VMReplicationRelationship => null;

	public IVMReplicationRelationship VMExtendedReplicationRelationship => null;

	public abstract Server Server { get; }

	public abstract WmiObjectPath ManagementPath { get; }

	public event EventHandler Deleted;

	public event EventHandler CacheUpdated;

	public VMComputerSystemOperationalStatus[] GetOperationalStatus()
	{
		return null;
	}

	public string[] GetStatusDescriptions()
	{
		return null;
	}

	public IVMTask BeginAddDevice(IVMDeviceSetting deviceToAdd)
	{
		return null;
	}

	public IVMDeviceSetting EndAddDevice(IVMTask addDeviceTask)
	{
		return null;
	}

	public IVMTask BeginSetState(VMComputerSystemState state)
	{
		return null;
	}

	public void EndSetState(IVMTask setStateTask)
	{
	}

	public IVMComputerSystemSetting GetPreviousSnapshot(bool needsRefresh)
	{
		return null;
	}

	public IVMReplicationSettingData GetReplicationSettingData(ReplicationRelationshipType relationshipType, bool throwIfNotFound)
	{
		return null;
	}

	public IVMReplicationRelationship GetReplicationRelationship(ReplicationRelationshipType relationshipType, bool throwIfNotFound)
	{
		return null;
	}

	public void RemoveFromCache()
	{
	}

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

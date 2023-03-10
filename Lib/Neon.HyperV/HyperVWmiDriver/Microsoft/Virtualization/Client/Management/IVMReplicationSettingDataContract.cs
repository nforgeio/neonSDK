using System;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IVMReplicationSettingDataContract : IVMReplicationSettingData, IVirtualizationManagementObject, IPutableAsync, IPutable
{
	public string InstanceId => null;

	public FailoverReplicationAuthenticationType AuthenticationType
	{
		get
		{
			return (FailoverReplicationAuthenticationType)0;
		}
		set
		{
		}
	}

	public bool BypassProxyServer
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public bool CompressionEnabled
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public string RecoveryConnectionPoint
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public string RecoveryHostSystem => null;

	public string PrimaryConnectionPoint => null;

	public string PrimaryHostSystem => null;

	public int RecoveryServerPort
	{
		get
		{
			return 0;
		}
		set
		{
		}
	}

	public string CertificateThumbPrint
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public int ApplicationConsistentSnapshotInterval
	{
		get
		{
			return 0;
		}
		set
		{
		}
	}

	public FailoverReplicationInterval ReplicationInterval
	{
		get
		{
			return (FailoverReplicationInterval)0;
		}
		set
		{
		}
	}

	public int RecoveryHistory
	{
		get
		{
			return 0;
		}
		set
		{
		}
	}

	public bool AutoResynchronizeEnabled
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public TimeSpan AutoResynchronizeIntervalStart
	{
		get
		{
			return default(TimeSpan);
		}
		set
		{
		}
	}

	public TimeSpan AutoResynchronizeIntervalEnd
	{
		get
		{
			return default(TimeSpan);
		}
		set
		{
		}
	}

	public WmiObjectPath[] IncludedDisks
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public bool EnableWriteOrderPreservationAcrossDisks
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public string ReplicationProvider
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public bool ReplicateHostKvpItems
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public IVMComputerSystemBase VMComputerSystem => null;

	public IVMReplicationRelationship Relationship => null;

	public abstract Server Server { get; }

	public abstract WmiObjectPath ManagementPath { get; }

	public event EventHandler Deleted;

	public event EventHandler CacheUpdated;

	public abstract IVMTask BeginPut();

	public abstract void EndPut(IVMTask putTask);

	public abstract void Put();

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

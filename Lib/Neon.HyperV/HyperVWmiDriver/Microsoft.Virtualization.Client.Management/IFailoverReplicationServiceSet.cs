using System;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IFailoverReplicationServiceSettingContract : IFailoverReplicationServiceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable
{
	public RecoveryAuthenticationType AllowedAuthenticationType
	{
		get
		{
			return RecoveryAuthenticationType.Undefined;
		}
		set
		{
		}
	}

	public string CertificateThumbprint
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public int HttpPort
	{
		get
		{
			return 0;
		}
		set
		{
		}
	}

	public int HttpsPort
	{
		get
		{
			return 0;
		}
		set
		{
		}
	}

	public uint MonitoringInterval
	{
		get
		{
			return 0u;
		}
		set
		{
		}
	}

	public TimeSpan MonitoringStartTime
	{
		get
		{
			return default(TimeSpan);
		}
		set
		{
		}
	}

	public bool RecoveryServerEnabled
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

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
[WmiName("Msvm_ReplicationServiceSettingData")]
internal interface IFailoverReplicationServiceSetting : IVirtualizationManagementObject, IPutableAsync, IPutable
{
	RecoveryAuthenticationType AllowedAuthenticationType { get; set; }

	string CertificateThumbprint { get; set; }

	int HttpPort { get; set; }

	int HttpsPort { get; set; }

	uint MonitoringInterval { get; set; }

	TimeSpan MonitoringStartTime { get; set; }

	bool RecoveryServerEnabled { get; set; }
}

using System;
using Microsoft.Virtualization.Client.Management.Clustering;

namespace Microsoft.Virtualization.Client.Management
{
	internal abstract class IMSClusterReplicaBrokerResourceContract : IMSClusterReplicaBrokerResource, IMSClusterResource, IMSClusterResourceBase, IVirtualizationManagementObject, IPutableAsync, IPutable
	{
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

		public string Authorization
		{
			get
			{
				return null;
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

		public string ListenerPortMapping
		{
			get
			{
				return null;
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

		public uint MonitoringStartTime
		{
			get
			{
				return 0u;
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

		public abstract string Name { get; }

		public abstract string Owner { get; }

		public abstract Server Server { get; }

		public abstract WmiObjectPath ManagementPath { get; }

		public event EventHandler Deleted;

		public event EventHandler CacheUpdated;

		public string GetCapName()
		{
			return null;
		}

		public abstract IMSClusterResourceGroup GetGroup();

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

		public abstract IVMTask BeginPut();

		public abstract void EndPut(IVMTask putTask);

		public abstract void Put();
	}
}
namespace Microsoft.Virtualization.Client.Management.Clustering
{
	[WmiName("MSCluster_Resource", PrimaryMapping = false)]
	internal interface IMSClusterReplicaBrokerResource : IMSClusterResource, IMSClusterResourceBase, IVirtualizationManagementObject, IPutableAsync, IPutable
	{
		FailoverReplicationAuthenticationType AuthenticationType { get; set; }

		string Authorization { get; set; }

		string CertificateThumbprint { get; set; }

		int HttpPort { get; set; }

		int HttpsPort { get; set; }

		string ListenerPortMapping { get; set; }

		uint MonitoringInterval { get; set; }

		uint MonitoringStartTime { get; set; }

		bool RecoveryServerEnabled { get; set; }

		string GetCapName();
	}
}

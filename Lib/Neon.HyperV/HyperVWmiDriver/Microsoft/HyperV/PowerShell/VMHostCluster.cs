using Microsoft.HyperV.PowerShell.Common;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Virtualization.Client.Management.Clustering;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMHostCluster : VirtualizationObject, IUpdatable
{
	private readonly DataUpdater<IMSClusterWmiProviderResource> m_ClusterWmiProvider;

	[VirtualizationObjectIdentifier(IdentifierFlags.UniqueIdentifier | IdentifierFlags.FriendlyName)]
	public string ClusterName => base.Server.Name;

	public string SharedStoragePath
	{
		get
		{
			return m_ClusterWmiProvider.GetData(UpdatePolicy.EnsureUpdated).ConfigStoreRootPath;
		}
		internal set
		{
			m_ClusterWmiProvider.GetData(UpdatePolicy.None).ConfigStoreRootPath = value;
		}
	}

	internal VMHostCluster(IMSClusterWmiProviderResource wmiProviderResource)
		: base(wmiProviderResource)
	{
		m_ClusterWmiProvider = InitializePrimaryDataUpdater(wmiProviderResource);
	}

	void IUpdatable.Put(IOperationWatcher operationWatcher)
	{
		operationWatcher.PerformPut(m_ClusterWmiProvider.GetData(UpdatePolicy.None), TaskDescriptions.SetVMHostCluster, this);
	}
}

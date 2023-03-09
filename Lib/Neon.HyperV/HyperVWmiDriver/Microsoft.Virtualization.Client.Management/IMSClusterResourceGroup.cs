using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management.Clustering;

[WmiName("MSCluster_ResourceGroup")]
internal interface IMSClusterResourceGroup : IVirtualizationManagementObject
{
	string Name { get; }

	IReadOnlyCollection<IMSClusterResource> GetResources();
}

using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management.Clustering;

[WmiName("MSCluster_Cluster")]
internal interface IMSClusterCluster : IVirtualizationManagementObject
{
	string Name { get; }

	string SharedVolumesRoot { get; }

	bool EnableSharedVolumes { get; }

	IEnumerable<IMSClusterNode> GetClusterNodes();

	IMSClusterReplicaBrokerResource GetReplicaBroker();

	IMSClusterVMResource GetVirtualMachineResource(string virtualMachineInstanceId);

	ClusterVerifyPathResult VerifyPath(string path, string groupName);
}

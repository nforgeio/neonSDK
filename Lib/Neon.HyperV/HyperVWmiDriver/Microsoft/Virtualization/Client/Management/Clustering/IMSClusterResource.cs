namespace Microsoft.Virtualization.Client.Management.Clustering;

[WmiName("MSCluster_Resource")]
internal interface IMSClusterResource : IMSClusterResourceBase, IVirtualizationManagementObject
{
	IMSClusterResourceGroup GetGroup();
}

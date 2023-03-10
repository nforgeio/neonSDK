namespace Microsoft.Virtualization.Client.Management.Clustering;

[WmiName("MSCluster_Resource", PrimaryMapping = false)]
internal interface IMSClusterCapResource : IMSClusterResource, IMSClusterResourceBase, IVirtualizationManagementObject
{
	string FullName { get; }
}

namespace Microsoft.Virtualization.Client.Management.Clustering;

[WmiName("MSCluster_Resource", PrimaryMapping = false)]
internal interface IMSClusterWmiProviderResource : IMSClusterResource, IMSClusterResourceBase, IVirtualizationManagementObject, IPutableAsync, IPutable
{
    string ConfigStoreRootPath { get; set; }
}

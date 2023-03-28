namespace Microsoft.Virtualization.Client.Management.Clustering;

[WmiName("MSCluster_Resource", PrimaryMapping = false)]
internal interface IMSClusterVMResource : IMSClusterVMOrConfigurationResource, IMSClusterResource, IMSClusterResourceBase, IVirtualizationManagementObject
{
}

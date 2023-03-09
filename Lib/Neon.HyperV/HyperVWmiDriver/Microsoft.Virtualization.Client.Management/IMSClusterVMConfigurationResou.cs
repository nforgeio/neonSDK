namespace Microsoft.Virtualization.Client.Management.Clustering;

[WmiName("MSCluster_Resource", PrimaryMapping = false)]
internal interface IMSClusterVMConfigurationResource : IMSClusterVMOrConfigurationResource, IMSClusterResource, IMSClusterResourceBase, IVirtualizationManagementObject
{
}

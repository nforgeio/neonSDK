namespace Microsoft.Virtualization.Client.Management.Clustering;

[WmiName("MSCluster_Resource", PrimaryMapping = false)]
internal interface IMSClusterVMOrConfigurationResource : IMSClusterResource, IMSClusterResourceBase, IVirtualizationManagementObject
{
    string VMComputerSystemInstanceId { get; }
}

namespace Microsoft.Virtualization.Client.Management.Clustering;

[WmiName("MSCluster_Node")]
internal interface IMSClusterNode : IVirtualizationManagementObject
{
    string Name { get; }

    IMSClusterCluster GetCluster();

    IMSClusterReplicaBrokerResource GetReplicaBroker();

    IMSClusterVMResource GetVirtualMachineResource(string virtualMachineInstanceId);
}

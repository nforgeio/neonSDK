namespace Microsoft.Virtualization.Client.Management.Clustering;

internal class MSClusterNodeView : MSClusterCommonView, IMSClusterNode, IVirtualizationManagementObject
{
    public IMSClusterCluster GetCluster()
    {
        return GetRelatedObject<IMSClusterCluster>(base.Associations.ClusterToNode);
    }
}

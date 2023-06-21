namespace Microsoft.Virtualization.Client.Management.Clustering;

internal sealed class MSClusterWmiProviderResourceView : MSClusterResourceView, IMSClusterWmiProviderResource, IMSClusterResource, IMSClusterResourceBase, IVirtualizationManagementObject, IPutableAsync, IPutable
{
    public string ConfigStoreRootPath
    {
        get
        {
            return (string)GetInternalProperty("ConfigStoreRootPath");
        }
        set
        {
            SetInternalProperty("ConfigStoreRootPath", value);
        }
    }
}

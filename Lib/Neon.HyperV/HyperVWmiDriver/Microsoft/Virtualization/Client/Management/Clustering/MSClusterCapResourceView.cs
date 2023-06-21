namespace Microsoft.Virtualization.Client.Management.Clustering;

internal sealed class MSClusterCapResourceView : MSClusterResourceView, IMSClusterCapResource, IMSClusterResource, IMSClusterResourceBase, IVirtualizationManagementObject
{
    private string DomainName => (string)GetInternalProperty("DnsSuffix");

    public string FullName => base.Name + "." + DomainName;
}

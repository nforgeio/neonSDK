namespace Microsoft.Virtualization.Client.Management.Clustering;

internal abstract class MSClusterVMOrConfigurationResourceView : MSClusterResourceView, IMSClusterVMOrConfigurationResource, IMSClusterResource, IMSClusterResourceBase, IVirtualizationManagementObject
{
    public string VMComputerSystemInstanceId => (string)GetInternalProperty("VmID");
}

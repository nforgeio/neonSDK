using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management.Clustering;

internal class MSClusterResourceGroupView : View, IMSClusterResourceGroup, IVirtualizationManagementObject
{
    internal static class WmiMemberNames
    {
        public const string Name = "Name";
    }

    public string Name => GetProperty<string>("Name");

    public IReadOnlyCollection<IMSClusterResource> GetResources()
    {
        return new List<IMSClusterResource>(GetRelatedObjects<IMSClusterResource>(base.Associations.ClusterGroupToResource)).AsReadOnly();
    }
}

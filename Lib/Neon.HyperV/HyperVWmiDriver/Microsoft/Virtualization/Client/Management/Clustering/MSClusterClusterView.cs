using System;

namespace Microsoft.Virtualization.Client.Management.Clustering;

internal class MSClusterClusterView : MSClusterCommonView, IMSClusterCluster, IVirtualizationManagementObject
{
    internal static class WmiMemberNames
    {
        public const string SharedVolumesRoot = "SharedVolumesRoot";

        public const string EnableSharedVolumes = "EnableSharedVolumes";

        public const string VerifyPath = "VerifyPath";
    }

    public string SharedVolumesRoot => GetProperty<string>("SharedVolumesRoot");

    public bool EnableSharedVolumes => Convert.ToBoolean(GetProperty<uint>("EnableSharedVolumes"));

    public ClusterVerifyPathResult VerifyPath(string path, string groupName)
    {
        object[] array = new object[3] { path, groupName, null };
        InvokeMethodWithoutReturn("VerifyPath", array);
        return (ClusterVerifyPathResult)array[2];
    }
}

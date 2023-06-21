namespace Microsoft.Virtualization.Client.Management.Clustering;

internal class MSClusterResourceBaseView : View
{
    internal static class WmiMemberNames
    {
        public const string Name = "Name";

        public const string Owner = "OwnerNode";

        public const string Type = "Type";

        public const string VMComputerSystemInstanceId = "VmID";

        public const string ConfigStoreRootPath = "ConfigStoreRootPath";

        public const string PrivateProperties = "PrivateProperties";

        public const string DnsSuffix = "DnsSuffix";
    }

    public string Name => GetProperty<string>("Name");

    public string Owner => GetProperty<string>("OwnerNode");
}

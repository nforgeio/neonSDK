namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_ReplicationAuthorizationSettingData", PrimaryMapping = false)]
internal class ReplicationAuthorizationEntry : EmbeddedInstance
{
    internal static class WmiPropertyNames
    {
        public const string AllowedPrimaryHostSystem = "AllowedPrimaryHostSystem";

        public const string ReplicaStorageLocation = "ReplicaStorageLocation";

        public const string TrustGroup = "TrustGroup";
    }

    public ReplicationAuthorizationEntry()
    {
    }

    public ReplicationAuthorizationEntry(Server server, string allowedPrimaryServer, string replicaStorageLocation, string trustGroup)
        : base(server, server.VirtualizationNamespace, "Msvm_ReplicationAuthorizationSettingData")
    {
        AddProperty("AllowedPrimaryHostSystem", allowedPrimaryServer);
        AddProperty("ReplicaStorageLocation", replicaStorageLocation);
        AddProperty("TrustGroup", trustGroup);
    }
}

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_ReplicationAuthorizationSettingData")]
internal interface IFailoverReplicationAuthorizationSetting : IVirtualizationManagementObject, IPutableAsync, IPutable, IDeleteableAsync, IDeleteable
{
	string AllowedPrimaryHostSystem { get; set; }

	string TrustGroup { get; set; }

	string ReplicaStorageLocation { get; set; }

	IReplicationService Service { get; }
}

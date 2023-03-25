namespace Microsoft.Virtualization.Client.Management.Clustering;

[WmiName("MSCluster_Resource", PrimaryMapping = false)]
internal interface IMSClusterReplicaBrokerResource : IMSClusterResource, IMSClusterResourceBase, IVirtualizationManagementObject, IPutableAsync, IPutable
{
	FailoverReplicationAuthenticationType AuthenticationType { get; set; }

	string Authorization { get; set; }

	string CertificateThumbprint { get; set; }

	int HttpPort { get; set; }

	int HttpsPort { get; set; }

	string ListenerPortMapping { get; set; }

	uint MonitoringInterval { get; set; }

	uint MonitoringStartTime { get; set; }

	bool RecoveryServerEnabled { get; set; }

	string GetCapName();
}

namespace Microsoft.Virtualization.Client.Management.Clustering;

internal enum ClusterVerifyPathResult : uint
{
	Valid = 0u,
	AvailableStorage = 1u,
	Network = 2u,
	NonCluster = 3u,
	NotValid = 128u,
	NotInGroup = 129u,
	OtherGroup = 130u
}

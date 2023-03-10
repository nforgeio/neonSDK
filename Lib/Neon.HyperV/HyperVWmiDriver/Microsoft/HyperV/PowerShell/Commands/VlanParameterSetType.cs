namespace Microsoft.HyperV.PowerShell.Commands;

internal enum VlanParameterSetType
{
	Unknown,
	MissingParameter,
	ConflictingParameters,
	Untagged,
	Access,
	Trunk,
	PrivateIsolated,
	PrivateCommunity,
	PrivatePromiscuous
}

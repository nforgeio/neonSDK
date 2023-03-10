namespace Microsoft.HyperV.PowerShell;

internal class VMResourcePoolPortAclMeteringReport : VMPortAclMeteringReport
{
	internal VMResourcePoolPortAclMeteringReport(string remoteAddress, VMNetworkAdapterAclDirection direction, ulong traffic)
		: base(string.Empty, remoteAddress, direction, traffic)
	{
	}
}

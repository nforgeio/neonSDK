namespace Microsoft.HyperV.PowerShell;

internal class VMPortAclMeteringReport
{
	public string LocalAddress { get; protected set; }

	public string RemoteAddress { get; protected set; }

	public VMNetworkAdapterAclDirection Direction { get; protected set; }

	public ulong TotalTraffic { get; protected set; }

	internal VMPortAclMeteringReport(string localAddress, string remoteAddress, VMNetworkAdapterAclDirection direction, ulong traffic)
	{
		LocalAddress = localAddress;
		RemoteAddress = remoteAddress;
		Direction = direction;
		TotalTraffic = traffic;
	}
}

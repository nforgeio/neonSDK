namespace Microsoft.HyperV.PowerShell;

internal class VMNetworkAdapterPortAclMeteringReport : VMPortAclMeteringReport
{
    public VMNetworkAdapter NetworkAdapter { get; protected set; }

    internal VMNetworkAdapterPortAclMeteringReport(VMNetworkAdapter adapter, string localAddress, string remoteAddress, VMNetworkAdapterAclDirection direction, ulong traffic)
        : base(localAddress, remoteAddress, direction, traffic)
    {
        NetworkAdapter = adapter;
    }
}

namespace Microsoft.HyperV.PowerShell.Commands;

internal interface IVMNetworkAdapterAclCmdlet
{
    string[] LocalIPAddress { get; }

    string[] LocalMacAddress { get; }

    string[] RemoteIPAddress { get; }

    string[] RemoteMacAddress { get; }
}

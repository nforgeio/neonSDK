namespace Microsoft.Virtualization.Client.Common;

internal abstract class IServerContract : IServer
{
    string IServer.Name => null;

    string IServer.FullName => null;

    HyperVOSVersion IServer.OSVersion => HyperVOSVersion.Unsupported;

    IWindowsCredential IServer.Credential => null;

    bool IServer.IsHyperVComponentSupported(HyperVComponent component, out string notSupportedReason)
    {
        notSupportedReason = null;
        return false;
    }
}

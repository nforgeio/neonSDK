namespace Microsoft.Virtualization.Client.Common;

internal interface IServer
{
    string Name { get; }

    string FullName { get; }

    HyperVOSVersion OSVersion { get; }

    IWindowsCredential Credential { get; }

    bool IsHyperVComponentSupported(HyperVComponent component, out string notSupportedReason);
}

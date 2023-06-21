namespace Microsoft.Virtualization.Client.Management;

internal enum IsolationMode : uint
{
    Unknown,
    NativeVirtualSubnetId,
    ExternalVirtualSubnetId,
    Vlan
}

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.HyperV.PowerShell;

[SuppressMessage("Microsoft.Design", "CA1028:EnumStorageShouldBeInt32", Justification = "This is consistent with MOF.")]
internal enum VMNetworkAdapterIsolationMode : byte
{
    None,
    NativeVirtualSubnet,
    ExternalVirtualSubnet,
    Vlan
}

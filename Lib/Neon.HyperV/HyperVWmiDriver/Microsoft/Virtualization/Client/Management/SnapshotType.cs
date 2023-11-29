namespace Microsoft.Virtualization.Client.Management;

internal enum SnapshotType : ushort
{
    Regular = 2,
    Recovery = 32768,
    Automatic = 34818
}

namespace Microsoft.Virtualization.Client.Management;

internal sealed class Win32ComputerSystemView : View, IWin32ComputerSystem, IVirtualizationManagementObject
{
    internal static class WmiPropertyNames
    {
        public const string Domain = "Domain";

        public const string TotalPhysicalMemory = "TotalPhysicalMemory";
    }

    public string Domain => GetProperty<string>("Domain");

    public long TotalPhysicalMemory => NumberConverter.UInt64ToInt64(GetProperty<ulong>("TotalPhysicalMemory"));
}

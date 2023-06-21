namespace Microsoft.Virtualization.Client.Management;

[WmiName("Win32_ComputerSystem")]
internal interface IWin32ComputerSystem : IVirtualizationManagementObject
{
    string Domain { get; }

    long TotalPhysicalMemory { get; }
}

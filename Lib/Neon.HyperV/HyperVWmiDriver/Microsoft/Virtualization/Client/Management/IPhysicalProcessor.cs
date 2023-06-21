namespace Microsoft.Virtualization.Client.Management;

[WmiName("Win32_Processor")]
internal interface IPhysicalProcessor : IVirtualizationManagementObject
{
    int NumberOfThreadsOfExecution { get; }
}

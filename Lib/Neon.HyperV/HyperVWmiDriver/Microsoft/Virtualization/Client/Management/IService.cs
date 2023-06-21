namespace Microsoft.Virtualization.Client.Management;

[WmiName("Win32_Service")]
internal interface IService : IVirtualizationManagementObject
{
    [Key]
    string Name { get; }

    bool Started { get; }

    ServiceState State { get; }

    void Start();

    void Stop();
}

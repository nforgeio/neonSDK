namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_VirtualSystemMigrationSettingData")]
internal interface IVMMigrationSetting : IVirtualizationManagementObject
{
    VMMigrationType MigrationType { get; set; }

    string DestinationPlannedVirtualSystemId { get; set; }

    string[] DestinationIPAddressList { get; set; }

    bool RetainVhdCopiesOnSource { get; set; }

    bool EnableCompression { get; set; }

    VMMigrationTransportType TransportType { get; set; }

    MoveUnmanagedVhd[] UnmanagedVhds { get; set; }

    bool RemoveSourceUnmanagedVhds { get; set; }
}

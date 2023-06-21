namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_VirtualSystemExportSettingData")]
internal interface IVMExportSetting : IVirtualizationManagementObject
{
    bool CopyVmStorage { get; set; }

    bool CopyVmRuntimeInformation { get; set; }

    bool CreateVmExportSubdirectory { get; set; }

    SnapshotExportMode CopySnapshotConfiguration { get; set; }

    IVMComputerSystemSetting SnapshotVirtualSystem { get; set; }

    CaptureLiveStateMode CaptureLiveState { get; set; }
}

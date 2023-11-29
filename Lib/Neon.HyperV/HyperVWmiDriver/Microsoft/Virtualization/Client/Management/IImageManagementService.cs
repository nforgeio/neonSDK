using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_ImageManagementService")]
internal interface IImageManagementService : IVirtualizationManagementObject
{
    IVMTask BeginCreateVirtualHardDisk(VirtualHardDiskType type, VirtualHardDiskFormat format, string path, string parentPath, long maxInternalSize);

    IVMTask BeginCreateVirtualHardDisk(VirtualHardDiskType type, VirtualHardDiskFormat format, string path, string parentPath, long maxInternalSize, long blockSize, long logicalSectorSize, long physicalSectorSize, bool pmemCompatible, VirtualHardDiskPmemAddressAbstractionType addressAbstractionType, long dataAlignment);

    void EndCreateVirtualHardDisk(IVMTask createVhdTask);

    IVMTask BeginCreateVirtualFloppyDisk(string path);

    void EndCreateVirtualFloppyDisk(IVMTask createVfdTask);

    IVMTask BeginReconnectParentVirtualHardDisk(string childPath, string parentPath, string leafPath, bool reconnectWithForce);

    void EndReconnectParentVirtualHardDisk(IVMTask reconnectTask);

    IVMTask BeginMergeVirtualHardDisk(string sourcePath, string destinationPath);

    void EndMergeVirtualHardDisk(IVMTask mergeVhdTask);

    IVMTask BeginCompactVirtualHardDisk(string path, VirtualHardDiskFormat format);

    IVMTask BeginCompactVirtualHardDisk(string path, uint mode);

    void EndCompactVirtualHardDisk(IVMTask compactVhdTask);

    IVMTask BeginResizeVirtualHardDisk(string path, ulong? maxInternalSize);

    void EndResizeVirtualHardDisk(IVMTask resizeVhdTask);

    IVMTask BeginConvertVirtualHardDisk(string sourcePath, string destinationPath, string destinationParentPath, VirtualHardDiskType destinationType, VirtualHardDiskFormat destinationFormat, long blockSize);

    IVMTask BeginConvertVirtualHardDisk(string sourcePath, string destinationPath, string destinationParentPath, VirtualHardDiskType destinationType, VirtualHardDiskFormat destinationFormat, long blockSize, bool pmemCompatible, VirtualHardDiskPmemAddressAbstractionType addressAbstractionType);

    void EndConvertVirtualHardDisk(IVMTask convertVhdTask);

    VirtualHardDiskSettingData GetVirtualHardDiskSettingData(string path);

    IVMTask BeginSetVirtualHardDiskSettingData(VirtualHardDiskSettingData settings);

    void EndSetVirtualHardDiskSettingData(IVMTask setVhdSettingDataTask);

    VirtualHardDiskState GetVirtualHardDiskState(string path);

    IVMTask BeginValidateVirtualHardDisk(string path);

    void EndValidateVirtualHardDisk(IVMTask validateTask);

    IVMTask BeginAttachVirtualHardDisk(string path, bool assignDriveLetter, bool readOnly);

    void EndAttachVirtualHardDisk(IVMTask attachTask);

    IVMTask BeginValidatePersistentReservationSupport(string path);

    void EndValidatePersistentReservationSupport(IVMTask validateTask);

    IMountedStorageImage GetMountedStorageImage(string path);

    IWin32DiskDrive GetWin32DiskDrive(uint diskNumber);

    VHDSetInformation GetVHDSetInformation(string vhdSetPath, bool getAllPaths);

    List<VHDSnapshotInformation> GetVHDSnapshotInformation(string vhdSetPath, List<string> snapshotIDs, bool getParentPathsList);

    IVMTask BeginSetVHDSnapshotInformation(VHDSnapshotInformation snapshot);

    void EndSetVHDSnapshotInformation(IVMTask setVHDSnapshotInformationTask);

    IVMTask BeginDeleteVHDSnapshot(string vhdSetPath, string snapshotId, bool persistReferenceSnapshot);

    void EndDeleteVHDSnapshot(IVMTask deleteVHDSnapshotTask);

    IVMTask BeginOptimizeVHDSet(string path);

    void EndOptimizeVHDSet(IVMTask optimizeVHDSetTask);
}

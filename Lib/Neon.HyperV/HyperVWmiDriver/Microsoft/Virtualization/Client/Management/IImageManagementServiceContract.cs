using System;
using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IImageManagementServiceContract : IImageManagementService, IVirtualizationManagementObject
{
    public abstract Server Server { get; }

    public abstract WmiObjectPath ManagementPath { get; }

    public event EventHandler Deleted;

    public event EventHandler CacheUpdated;

    public IVMTask BeginCreateVirtualHardDisk(VirtualHardDiskType type, VirtualHardDiskFormat format, string path, string parentPath, long maxInternalSize)
    {
        return null;
    }

    public IVMTask BeginCreateVirtualHardDisk(VirtualHardDiskType type, VirtualHardDiskFormat format, string path, string parentPath, long maxInternalSize, long blockSize, long logicalSectorSize, long physicalSectorSize, bool pmemCompatible, VirtualHardDiskPmemAddressAbstractionType pmemAddressAbstractionType, long dataAlignment)
    {
        return null;
    }

    public void EndCreateVirtualHardDisk(IVMTask createVhdTask)
    {
    }

    public IVMTask BeginCreateVirtualFloppyDisk(string path)
    {
        return null;
    }

    public void EndCreateVirtualFloppyDisk(IVMTask createVfdTask)
    {
    }

    public IVMTask BeginReconnectParentVirtualHardDisk(string childPath, string parentPath, string leafPath, bool reconnectWithForce)
    {
        return null;
    }

    public void EndReconnectParentVirtualHardDisk(IVMTask reconnectTask)
    {
    }

    public IVMTask BeginMergeVirtualHardDisk(string sourcePath, string destinationPath)
    {
        return null;
    }

    public void EndMergeVirtualHardDisk(IVMTask mergeVhdTask)
    {
    }

    public IVMTask BeginCompactVirtualHardDisk(string path, VirtualHardDiskFormat format)
    {
        return null;
    }

    public IVMTask BeginCompactVirtualHardDisk(string path, uint mode)
    {
        return null;
    }

    public void EndCompactVirtualHardDisk(IVMTask compactVhdTask)
    {
    }

    public IVMTask BeginResizeVirtualHardDisk(string path, ulong? maxInternalSize)
    {
        return null;
    }

    public void EndResizeVirtualHardDisk(IVMTask resizeVhdTask)
    {
    }

    public IVMTask BeginConvertVirtualHardDisk(string sourcePath, string destinationPath, string destinationParentPath, VirtualHardDiskType destinationType, VirtualHardDiskFormat destinationFormat, long blockSize)
    {
        return null;
    }

    public IVMTask BeginConvertVirtualHardDisk(string sourcePath, string destinationPath, string destinationParentPath, VirtualHardDiskType destinationType, VirtualHardDiskFormat destinationFormat, long blockSize, bool pmemCompatible, VirtualHardDiskPmemAddressAbstractionType addressAbstractionType)
    {
        return null;
    }

    public void EndConvertVirtualHardDisk(IVMTask convertVhdTask)
    {
    }

    public VirtualHardDiskSettingData GetVirtualHardDiskSettingData(string path)
    {
        return null;
    }

    public IVMTask BeginSetVirtualHardDiskSettingData(VirtualHardDiskSettingData settings)
    {
        return null;
    }

    public void EndSetVirtualHardDiskSettingData(IVMTask setVhdSettingDataTask)
    {
    }

    public VirtualHardDiskState GetVirtualHardDiskState(string path)
    {
        return null;
    }

    public IVMTask BeginValidateVirtualHardDisk(string path)
    {
        return null;
    }

    public void EndValidateVirtualHardDisk(IVMTask validateTask)
    {
    }

    public IVMTask BeginAttachVirtualHardDisk(string path, bool assignDriveLetter, bool readOnly)
    {
        return null;
    }

    public void EndAttachVirtualHardDisk(IVMTask attachTask)
    {
    }

    public IVMTask BeginValidatePersistentReservationSupport(string path)
    {
        return null;
    }

    public void EndValidatePersistentReservationSupport(IVMTask validateTask)
    {
    }

    public IMountedStorageImage GetMountedStorageImage(string path)
    {
        return null;
    }

    public IWin32DiskDrive GetWin32DiskDrive(uint diskNumber)
    {
        return null;
    }

    public VHDSetInformation GetVHDSetInformation(string vhdSetPath, bool getAllPaths)
    {
        return null;
    }

    public List<VHDSnapshotInformation> GetVHDSnapshotInformation(string vhdSetPath, List<string> snapshotIDs, bool getParentPathsList)
    {
        return null;
    }

    public IVMTask BeginSetVHDSnapshotInformation(VHDSnapshotInformation snapshot)
    {
        return null;
    }

    public void EndSetVHDSnapshotInformation(IVMTask setVHDSnapshotInformationTask)
    {
    }

    public IVMTask BeginDeleteVHDSnapshot(string vhdSetPath, string snapshotId, bool persistReferenceSnapshot)
    {
        return null;
    }

    public void EndDeleteVHDSnapshot(IVMTask deleteVHDSnapshotTask)
    {
    }

    public IVMTask BeginOptimizeVHDSet(string path)
    {
        return null;
    }

    public void EndOptimizeVHDSet(IVMTask optimizeVHDSetTask)
    {
    }

    public abstract void InvalidatePropertyCache();

    public abstract void UpdatePropertyCache();

    public abstract void UpdatePropertyCache(TimeSpan threshold);

    public abstract void RegisterForInstanceModificationEvents(InstanceModificationEventStrategy strategy);

    public abstract void UnregisterForInstanceModificationEvents();

    public abstract void InvalidateAssociationCache();

    public abstract void UpdateAssociationCache();

    public abstract void UpdateAssociationCache(TimeSpan threshold);

    public abstract string GetEmbeddedInstance();

    public abstract void DiscardPendingPropertyChanges();
}

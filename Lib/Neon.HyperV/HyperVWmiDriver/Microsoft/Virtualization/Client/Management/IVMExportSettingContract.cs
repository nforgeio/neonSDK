using System;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IVMExportSettingContract : IVMExportSetting, IVirtualizationManagementObject
{
    public bool CopyVmStorage
    {
        get
        {
            return false;
        }
        set
        {
        }
    }

    public bool CopyVmRuntimeInformation
    {
        get
        {
            return false;
        }
        set
        {
        }
    }

    public bool CreateVmExportSubdirectory
    {
        get
        {
            return false;
        }
        set
        {
        }
    }

    public SnapshotExportMode CopySnapshotConfiguration
    {
        get
        {
            return SnapshotExportMode.ExportAllSnapshots;
        }
        set
        {
        }
    }

    public IVMComputerSystemSetting SnapshotVirtualSystem
    {
        get
        {
            return null;
        }
        set
        {
        }
    }

    public CaptureLiveStateMode CaptureLiveState
    {
        get
        {
            return CaptureLiveStateMode.CaptureCrashConsistentState;
        }
        set
        {
        }
    }

    public abstract Server Server { get; }

    public abstract WmiObjectPath ManagementPath { get; }

    public event EventHandler Deleted;

    public event EventHandler CacheUpdated;

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

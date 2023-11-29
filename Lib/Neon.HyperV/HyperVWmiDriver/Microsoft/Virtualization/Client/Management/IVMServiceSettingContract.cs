using System;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IVMServiceSettingContract : IVMServiceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable
{
    public string DefaultExternalDataRoot
    {
        get
        {
            return null;
        }
        set
        {
        }
    }

    public string DefaultVirtualHardDiskPath
    {
        get
        {
            return null;
        }
        set
        {
        }
    }

    public string MinimumMacAddress
    {
        get
        {
            return null;
        }
        set
        {
        }
    }

    public string MaximumMacAddress
    {
        get
        {
            return null;
        }
        set
        {
        }
    }

    public string MinimumWorldWidePortName
    {
        get
        {
            return null;
        }
        set
        {
        }
    }

    public string MaximumWorldWidePortName
    {
        get
        {
            return null;
        }
        set
        {
        }
    }

    public string AssignedWorldWideNodeName
    {
        get
        {
            return null;
        }
        set
        {
        }
    }

    public bool NumaSpanningEnabled
    {
        get
        {
            return false;
        }
        set
        {
        }
    }

    public bool NumaSpanningSupported => false;

    public bool EnhancedSessionModeEnabled
    {
        get
        {
            return false;
        }
        set
        {
        }
    }

    public bool EnhancedSessionModeSupported => false;

    public bool HypervisorRootSchedulerEnabled
    {
        get
        {
            return false;
        }
        set
        {
        }
    }

    public abstract Server Server { get; }

    public abstract WmiObjectPath ManagementPath { get; }

    public event EventHandler Deleted;

    public event EventHandler CacheUpdated;

    public abstract IVMTask BeginPut();

    public abstract void EndPut(IVMTask putTask);

    public abstract void Put();

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

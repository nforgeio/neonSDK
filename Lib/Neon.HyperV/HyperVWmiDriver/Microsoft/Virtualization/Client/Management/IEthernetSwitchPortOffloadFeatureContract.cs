using System;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IEthernetSwitchPortOffloadFeatureContract : IEthernetSwitchPortOffloadFeature, IEthernetSwitchPortFeature, IEthernetFeature, IVirtualizationManagementObject
{
    public int VMQOffloadWeight
    {
        get
        {
            return 0;
        }
        set
        {
        }
    }

    public int IOVOffloadWeight
    {
        get
        {
            return 0;
        }
        set
        {
        }
    }

    public int IPSecOffloadLimit
    {
        get
        {
            return 0;
        }
        set
        {
        }
    }

    public uint IOVQueuePairsRequested
    {
        get
        {
            return 0u;
        }
        set
        {
        }
    }

    public IovInterruptModerationMode InterruptModerationMode
    {
        get
        {
            return IovInterruptModerationMode.Unknown;
        }
        set
        {
        }
    }

    public uint PacketDirectNumProcs
    {
        get
        {
            return 0u;
        }
        set
        {
        }
    }

    public uint PacketDirectModerationCount
    {
        get
        {
            return 0u;
        }
        set
        {
        }
    }

    public uint PacketDirectModerationInterval
    {
        get
        {
            return 0u;
        }
        set
        {
        }
    }

    public bool VrssEnabled
    {
        get
        {
            return true;
        }
        set
        {
        }
    }

    public bool VmmqEnabled
    {
        get
        {
            return true;
        }
        set
        {
        }
    }

    public uint VmmqQueuePairs
    {
        get
        {
            return 0u;
        }
        set
        {
        }
    }

    public uint VrssMinQueuePairs
    {
        get
        {
            return 0u;
        }
        set
        {
        }
    }

    public uint VrssQueueSchedulingMode
    {
        get
        {
            return 0u;
        }
        set
        {
        }
    }

    public bool VrssExcludePrimaryProcessor
    {
        get
        {
            return false;
        }
        set
        {
        }
    }

    public bool VrssIndependentHostSpreading
    {
        get
        {
            return false;
        }
        set
        {
        }
    }

    public uint VrssVmbusChannelAffinityPolicy
    {
        get
        {
            return 0u;
        }
        set
        {
        }
    }

    public abstract string InstanceId { get; }

    public abstract EthernetFeatureType FeatureType { get; }

    public abstract string Name { get; }

    public abstract string ExtensionId { get; }

    public abstract string FeatureId { get; }

    public abstract Server Server { get; }

    public abstract WmiObjectPath ManagementPath { get; }

    public event EventHandler Deleted;

    public event EventHandler CacheUpdated;

    public IVMTask BeginModifySingleFeature(IEthernetSwitchFeatureService service)
    {
        return null;
    }

    public void EndModifySingleFeature(IVMTask modifyTask)
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

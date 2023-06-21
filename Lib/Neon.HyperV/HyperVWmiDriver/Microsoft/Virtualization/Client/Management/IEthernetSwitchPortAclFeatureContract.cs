using System;
using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IEthernetSwitchPortAclFeatureContract : IEthernetSwitchPortAclFeature, IEthernetSwitchPortFeature, IEthernetFeature, IVirtualizationManagementObject, IMetricMeasurableElement
{
    public AclAction Action
    {
        get
        {
            return AclAction.Unknown;
        }
        set
        {
        }
    }

    public AclDirection Direction
    {
        get
        {
            return AclDirection.Unknown;
        }
        set
        {
        }
    }

    public string LocalAddress
    {
        get
        {
            return null;
        }
        set
        {
        }
    }

    public byte LocalAddressPrefixLength
    {
        get
        {
            return 0;
        }
        set
        {
        }
    }

    public string RemoteAddress
    {
        get
        {
            return null;
        }
        set
        {
        }
    }

    public byte RemoteAddressPrefixLength
    {
        get
        {
            return 0;
        }
        set
        {
        }
    }

    public bool IsRemote
    {
        get
        {
            return false;
        }
        set
        {
        }
    }

    public AclAddressType AddressType
    {
        get
        {
            return AclAddressType.Unknown;
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

    public abstract MetricEnabledState AggregateMetricEnabledState { get; }

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

    public abstract IReadOnlyCollection<IMetricValue> GetMetricValues();
}

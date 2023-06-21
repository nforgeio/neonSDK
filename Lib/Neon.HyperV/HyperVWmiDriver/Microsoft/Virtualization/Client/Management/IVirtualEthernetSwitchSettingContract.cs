using System;
using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IVirtualEthernetSwitchSettingContract : IVirtualEthernetSwitchSetting, IVirtualizationManagementObject, IPutableAsync, IPutable
{
    public string InstanceId => null;

    public Guid Id => default(Guid);

    public string SwitchFriendlyName
    {
        get
        {
            return null;
        }
        set
        {
        }
    }

    public string Notes
    {
        get
        {
            return null;
        }
        set
        {
        }
    }

    public bool IOVPreferred
    {
        get
        {
            return false;
        }
        set
        {
        }
    }

    public IList<IEthernetSwitchExtension> ExtensionList
    {
        get
        {
            return null;
        }
        set
        {
        }
    }

    public IVirtualEthernetSwitch Switch => null;

    public BandwidthReservationMode BandwidthReservationMode => BandwidthReservationMode.Default;

    public bool PacketDirectEnabled => false;

    public bool TeamingEnabled => false;

    public bool NATEnabled => false;

    public string NATSubnetAddress => null;

    public byte NATPrefixLength => 0;

    public IEnumerable<IEthernetSwitchFeature> Features => null;

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

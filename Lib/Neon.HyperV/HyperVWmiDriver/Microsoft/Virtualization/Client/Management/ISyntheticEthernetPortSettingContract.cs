using System;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class ISyntheticEthernetPortSettingContract : ISyntheticEthernetPortSetting, IEthernetPortSetting, IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable, IDeleteableAsync, IDeleteable
{
    public IFailoverNetworkAdapterSetting FailoverNetworkAdapterSetting => null;

    public bool DeviceNamingEnabled
    {
        get
        {
            return false;
        }
        set
        {
        }
    }

    public abstract bool IsNetworkAddressStatic { get; set; }

    public abstract string NetworkAddress { get; set; }

    public abstract IEthernetPort EthernetDevice { get; }

    public abstract bool ClusterMonitored { get; set; }

    public abstract IVMBootEntry BootEntry { get; }

    public abstract bool AllowPacketDirect { get; set; }

    public abstract uint MediaType { get; }

    public abstract bool InterruptModeration { get; set; }

    public abstract bool NumaAwarePlacement { get; set; }

    public abstract string FriendlyName { get; set; }

    public abstract string DeviceTypeName { get; }

    public abstract string DeviceId { get; }

    public abstract string PoolId { get; set; }

    public abstract Guid VMBusChannelInstanceGuid { get; set; }

    public abstract VMDeviceSettingType VMDeviceSettingType { get; }

    public abstract IVMDevice VirtualDevice { get; }

    public abstract IVMComputerSystemSetting VirtualComputerSystemSetting { get; }

    public abstract Server Server { get; }

    public abstract WmiObjectPath ManagementPath { get; }

    public event EventHandler Deleted;

    public event EventHandler CacheUpdated;

    public abstract IEthernetConnectionAllocationRequest GetConnectionConfiguration();

    public abstract IGuestNetworkAdapterConfiguration GetGuestNetworkAdapterConfiguration();

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

    public abstract IVMTask BeginDelete();

    public abstract void EndDelete(IVMTask deleteTask);

    public abstract void Delete();
}

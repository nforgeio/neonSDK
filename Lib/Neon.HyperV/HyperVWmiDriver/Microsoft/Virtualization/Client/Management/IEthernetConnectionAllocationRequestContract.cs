using System;
using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IEthernetConnectionAllocationRequestContract : IEthernetConnectionAllocationRequest, IEthernetPortAllocationSettingData, IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable, IDeleteableAsync, IDeleteable
{
    public IEthernetPortSetting Parent
    {
        get
        {
            return null;
        }
        set
        {
        }
    }

    public bool IsEnabled
    {
        get
        {
            return false;
        }
        set
        {
        }
    }

    public IResourcePool ResourcePool => null;

    public IReadOnlyList<string> RequiredFeatureIds
    {
        get
        {
            return null;
        }
        set
        {
        }
    }

    public IReadOnlyList<string> RequiredFeatureNames => null;

    public abstract WmiObjectPath[] HostResources { get; set; }

    public abstract WmiObjectPath HostResource { get; set; }

    public abstract string Address { get; set; }

    public abstract string TestReplicaPoolId { get; set; }

    public abstract string TestReplicaSwitchName { get; set; }

    public abstract IEnumerable<IEthernetSwitchPortFeature> Features { get; }

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

    public int TestNetworkConnectivity(bool isSender, string senderIPAddress, string receiverIPAddress, string receiverMacAddress, int isolationID, int sequenceNumber, int payloadSize)
    {
        return 0;
    }

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

namespace Microsoft.Virtualization.Client.Management;

internal interface IEthernetPortSetting : IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable, IDeleteableAsync, IDeleteable
{
    bool IsNetworkAddressStatic { get; set; }

    string NetworkAddress { get; set; }

    IEthernetPort EthernetDevice { get; }

    bool ClusterMonitored { get; set; }

    IVMBootEntry BootEntry { get; }

    IEthernetConnectionAllocationRequest GetConnectionConfiguration();

    IGuestNetworkAdapterConfiguration GetGuestNetworkAdapterConfiguration();
}

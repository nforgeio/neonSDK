namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_SyntheticEthernetPortSettingData")]
internal interface ISyntheticEthernetPortSetting : IEthernetPortSetting, IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable, IDeleteableAsync, IDeleteable
{
    IFailoverNetworkAdapterSetting FailoverNetworkAdapterSetting { get; }

    bool DeviceNamingEnabled { get; set; }

    uint MediaType { get; }

    bool AllowPacketDirect { get; set; }

    bool InterruptModeration { get; set; }

    bool NumaAwarePlacement { get; set; }
}

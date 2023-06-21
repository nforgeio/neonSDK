using System;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_ResourceAllocationSettingData")]
internal interface IVMDeviceSetting : IVirtualizationManagementObject, IPutableAsync, IPutable
{
    string FriendlyName { get; set; }

    string DeviceTypeName { get; }

    [Key]
    string DeviceId { get; }

    string PoolId { get; set; }

    Guid VMBusChannelInstanceGuid { get; set; }

    VMDeviceSettingType VMDeviceSettingType { get; }

    IVMDevice VirtualDevice { get; }

    IVMComputerSystemSetting VirtualComputerSystemSetting { get; }
}

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_ResourcePoolConfigurationService")]
internal interface IResourcePoolConfigurationService : IVirtualizationManagementObject
{
    IResourcePoolSetting CreateTemplatePoolSetting(string poolId, VMDeviceSettingType deviceType);

    IResourcePoolAllocationSetting CreateTemplateAllocationSetting(string poolId, VMDeviceSettingType deviceType);

    IVMTask BeginCreateResourcePool(IResourcePoolSetting resourcePoolSettingData, IResourcePool[] parentPools, IResourcePoolAllocationSetting[] resourceSettings);

    IResourcePool EndCreateResourcePool(IVMTask task);

    IVMTask BeginModifyResourcePool(IResourcePool childPool, IResourcePool[] parentPools, IResourcePoolAllocationSetting[] resourceSettings);

    void EndModifyResourcePool(IVMTask task);
}

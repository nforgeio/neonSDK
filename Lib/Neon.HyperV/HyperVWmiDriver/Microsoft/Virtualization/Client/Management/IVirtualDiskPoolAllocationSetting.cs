using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_StorageAllocationSettingData", PrimaryMapping = false)]
internal interface IVirtualDiskPoolAllocationSetting : IResourcePoolAllocationSetting, IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable
{
    bool HasBaseOfStoragePath(string path);

    IEnumerable<string> GetStoragePaths();

    void SetStoragePaths(IList<string> paths);
}

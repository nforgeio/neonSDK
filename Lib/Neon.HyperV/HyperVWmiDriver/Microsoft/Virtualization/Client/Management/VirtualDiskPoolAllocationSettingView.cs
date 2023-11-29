using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Virtualization.Client.Management;

internal class VirtualDiskPoolAllocationSettingView : ResourcePoolAllocationSettingView, IVirtualDiskPoolAllocationSetting, IResourcePoolAllocationSetting, IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable
{
    protected string[] StoragePaths
    {
        get
        {
            return GetProperty<string[]>("HostResource");
        }
        set
        {
            if (value == null)
            {
                value = new string[1] { string.Empty };
            }
            SetProperty("HostResource", value);
        }
    }

    public bool HasBaseOfStoragePath(string path)
    {
        string[] storagePaths = StoragePaths;
        if (string.IsNullOrEmpty(path) || storagePaths == null)
        {
            return false;
        }
        return storagePaths.Any((string h) => path.StartsWith(h, StringComparison.OrdinalIgnoreCase));
    }

    public IEnumerable<string> GetStoragePaths()
    {
        return StoragePaths ?? new string[0];
    }

    public void SetStoragePaths(IList<string> paths)
    {
        StoragePaths = paths?.ToArray();
    }
}

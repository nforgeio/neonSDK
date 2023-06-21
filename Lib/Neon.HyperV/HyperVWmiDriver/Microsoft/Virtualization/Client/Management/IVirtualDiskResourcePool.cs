using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_ResourcePool", PrimaryMapping = false)]
internal interface IVirtualDiskResourcePool : IResourcePool, IVirtualizationManagementObject, IDeleteableAsync, IDeleteable, IMetricMeasurableElement
{
    bool HasBaseOfStoragePath(string path);

    List<string> GetStoragePaths();

    IEnumerable<IVirtualDisk> GetAllocatedVirtualDisks();
}

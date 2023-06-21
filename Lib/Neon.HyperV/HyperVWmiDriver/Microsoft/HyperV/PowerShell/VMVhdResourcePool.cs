using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMVhdResourcePool : VMStorageResourcePool, IMeasurableResourcePool, IMeasurable, IMeasurableInternal, IVMResourcePool
{
    internal VMVhdResourcePool(IResourcePool resourcePool)
        : base(resourcePool)
    {
    }
}

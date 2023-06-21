using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMMemoryResourcePool : VMResourcePool, IMeasurableResourcePool, IMeasurable, IMeasurableInternal, IVMResourcePool
{
    internal VMMemoryResourcePool(IResourcePool resourcePool)
        : base(resourcePool)
    {
    }
}

using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMIsoResourcePool : VMStorageResourcePool
{
    internal VMIsoResourcePool(IResourcePool resourcePool)
        : base(resourcePool)
    {
    }
}

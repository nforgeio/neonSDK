using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMFcResourcePool : VMResourcePool
{
    internal VMFcResourcePool(IResourcePool resourcePool)
        : base(resourcePool)
    {
    }
}

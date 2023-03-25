using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMVfdResourcePool : VMStorageResourcePool
{
	internal VMVfdResourcePool(IResourcePool resourcePool)
		: base(resourcePool)
	{
	}
}

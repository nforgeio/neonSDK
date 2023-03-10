using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMProcessorResourcePool : VMResourcePool, IMeasurableResourcePool, IMeasurable, IMeasurableInternal, IVMResourcePool
{
	internal VMProcessorResourcePool(IResourcePool resourcePool)
		: base(resourcePool)
	{
	}
}

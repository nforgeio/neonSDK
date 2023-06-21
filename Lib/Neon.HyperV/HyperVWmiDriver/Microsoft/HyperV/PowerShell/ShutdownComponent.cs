using Microsoft.HyperV.PowerShell.Common;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class ShutdownComponent : VMIntegrationComponent
{
    internal override string PutDescription => TaskDescriptions.SetVMShutdownComponent;

    internal ShutdownComponent(IVMShutdownComponentSetting setting, VirtualMachineBase parentVirtualMachineObject)
        : base(setting, parentVirtualMachineObject)
    {
    }
}

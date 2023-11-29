using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_VirtualFcSwitch")]
internal interface IVirtualFcSwitch : IVirtualSwitch, IVirtualizationManagementObject, IPutable
{
    IEnumerable<IVirtualFcSwitchPort> SwitchPorts { get; }

    IExternalFcPort GetExternalFcPort();
}

using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal class VirtualFcSwitchView : VirtualSwitchView, IVirtualFcSwitch, IVirtualSwitch, IVirtualizationManagementObject, IPutable
{
    public IEnumerable<IVirtualFcSwitchPort> SwitchPorts => GetRelatedObjects<IVirtualFcSwitchPort>(base.Associations.FcSwitchToSwitchPort);

    public IExternalFcPort GetExternalFcPort()
    {
        IExternalFcPort externalFcPort = null;
        foreach (IVirtualFcSwitchPort switchPort in SwitchPorts)
        {
            IFcEndpoint fcEndpoint = switchPort.FcEndpoint;
            if (fcEndpoint == null)
            {
                continue;
            }
            IFcEndpoint otherEndpoint = fcEndpoint.OtherEndpoint;
            if (otherEndpoint != null)
            {
                IExternalFcPort externalFcPort2 = otherEndpoint.ExternalFcPort;
                if (externalFcPort2 != null)
                {
                    externalFcPort = externalFcPort2;
                    break;
                }
            }
        }
        if (externalFcPort == null)
        {
            throw ThrowHelper.CreateRelatedObjectNotFoundException(base.Server, typeof(IExternalFcPort));
        }
        return externalFcPort;
    }
}

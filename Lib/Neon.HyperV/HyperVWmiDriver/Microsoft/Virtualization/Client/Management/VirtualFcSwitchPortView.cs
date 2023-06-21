namespace Microsoft.Virtualization.Client.Management;

internal class VirtualFcSwitchPortView : VirtualSwitchPortView, IVirtualFcSwitchPort, IVirtualSwitchPort, IVirtualizationManagementObject
{
    public IFcEndpoint FcEndpoint => GetRelatedObject<IFcEndpoint>(base.Associations.FcSwitchPortToEndpointAssociation, throwIfNotFound: false);

    public IVirtualFcSwitch VirtualFcSwitch => GetRelatedObject<IVirtualFcSwitch>(base.Associations.FcSwitchPortToSwitch, throwIfNotFound: false);
}

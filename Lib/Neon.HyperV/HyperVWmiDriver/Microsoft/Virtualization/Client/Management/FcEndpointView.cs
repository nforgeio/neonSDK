namespace Microsoft.Virtualization.Client.Management;

internal class FcEndpointView : View, IFcEndpoint, IVirtualizationManagementObject
{
    public IExternalFcPort ExternalFcPort => GetRelatedObject<IExternalFcPort>(base.Associations.FcEndpointToExternalFcPortAssociation, throwIfNotFound: false);

    public IVirtualFcSwitchPort SwitchPort => GetRelatedObject<IVirtualFcSwitchPort>(base.Associations.FcEndpointToSwitchPortAssociation, throwIfNotFound: false);

    public IFcEndpoint OtherEndpoint => GetRelatedObject<IFcEndpoint>(base.Associations.FcEndpointToEndpointAssociation, throwIfNotFound: false);
}

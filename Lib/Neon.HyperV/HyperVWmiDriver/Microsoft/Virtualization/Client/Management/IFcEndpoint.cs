namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_FcEndpoint")]
internal interface IFcEndpoint : IVirtualizationManagementObject
{
    IExternalFcPort ExternalFcPort { get; }

    IVirtualFcSwitchPort SwitchPort { get; }

    IFcEndpoint OtherEndpoint { get; }
}

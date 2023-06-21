namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_ExternalFcPort")]
internal interface IExternalFcPort : IVMDevice, IVirtualizationManagementObject
{
    string WorldWideNodeName { get; }

    string WorldWidePortName { get; }

    bool IsHyperVCapable { get; }

    int OperationalStatus { get; }

    IFcEndpoint FcEndpoint { get; }

    IVirtualFcSwitch GetVirtualFcSwitch();
}

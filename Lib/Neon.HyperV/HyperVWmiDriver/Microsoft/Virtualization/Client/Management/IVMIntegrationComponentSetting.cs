namespace Microsoft.Virtualization.Client.Management;

internal interface IVMIntegrationComponentSetting : IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable
{
    bool Enabled { get; set; }

    IVMIntegrationComponent GetIntegrationComponent();
}

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_SettingsDefineCapabilities")]
internal interface ISettingsDefineCapabilities : IVirtualizationManagementObject
{
    IVirtualizationManagementObject PartComponent { get; }

    CapabilitiesValueRole ValueRole { get; }

    CapabilitiesValueRange ValueRange { get; }

    CapabilitiesSupportStatement SupportStatement { get; }
}

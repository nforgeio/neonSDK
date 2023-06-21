namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_BootSourceSettingData")]
internal sealed class BootSourceSetting : EmbeddedInstance
{
    private static class WmiPropertyNames
    {
        public const string BootSourceType = "BootSourceType";

        public const string OtherLocation = "OtherLocation";

        public const string FirmwareDevicePath = "FirmwareDevicePath";

        public const string BootSourceDescription = "BootSourceDescription";
    }

    public BootEntryType BootSourceType => (BootEntryType)GetProperty("BootSourceType", 0u);

    public string OtherLocation => GetProperty<string>("OtherLocation");

    public string FirmwareDevicePath => GetProperty<string>("FirmwareDevicePath");

    public string BootSourceDescription => GetProperty<string>("BootSourceDescription");

    public BootSourceSetting()
    {
    }

    public BootSourceSetting(Server server, BootEntryType bootSourceType, string otherLocation, string firmwareDevicePath, string bootSourceDescription)
        : base(server, server.VirtualizationNamespace, "Msvm_BootSourceSettingData")
    {
        AddProperty("BootSourceType", (uint)bootSourceType);
        AddProperty("OtherLocation", otherLocation);
        AddProperty("FirmwareDevicePath", firmwareDevicePath);
        AddProperty("BootSourceDescription", bootSourceDescription);
    }
}

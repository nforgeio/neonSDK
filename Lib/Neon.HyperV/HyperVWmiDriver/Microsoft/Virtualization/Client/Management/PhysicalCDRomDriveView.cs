namespace Microsoft.Virtualization.Client.Management;

internal class PhysicalCDRomDriveView : View, IPhysicalCDRomDrive, IVirtualizationManagementObject
{
    internal static class WmiPropertyNames
    {
        public const string DeviceId = "DeviceID";

        public const string Drive = "Drive";

        public const string PnpDeviceId = "PNPDeviceID";
    }

    public string DeviceId => GetProperty<string>("DeviceID");

    public string Drive => GetProperty<string>("Drive");

    public string PnpDeviceId => GetProperty<string>("PNPDeviceID");
}

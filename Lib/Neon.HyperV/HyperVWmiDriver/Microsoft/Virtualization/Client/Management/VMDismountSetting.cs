using System;
using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_AssignableDeviceDismountSettingData")]
internal class VMDismountSetting
{
    internal static class WmiPropertyNames
    {
        public const string DeviceInstancePath = "DeviceInstancePath";

        public const string DeviceLocationPath = "DeviceLocationPath";

        public const string RequireAcsSupport = "RequireAcsSupport";

        public const string RequireDeviceMitigations = "RequireDeviceMitigations";
    }

    public string DeviceInstancePath { get; set; }

    public string DeviceLocationPath { get; set; }

    public bool RequireAcsSupport { get; set; }

    public bool RequireDeviceMitigations { get; set; }

    public string GetSettingDataEmbeddedInstance(Server server)
    {
        if (server == null)
        {
            throw new ArgumentNullException("server");
        }
        Dictionary<string, object> dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        dictionary.Add("DeviceInstancePath", DeviceInstancePath);
        dictionary.Add("DeviceLocationPath", DeviceLocationPath);
        dictionary.Add("RequireAcsSupport", RequireAcsSupport);
        dictionary.Add("RequireDeviceMitigations", RequireDeviceMitigations);
        return server.GetNewEmbeddedInstance("Msvm_AssignableDeviceDismountSettingData", dictionary);
    }
}

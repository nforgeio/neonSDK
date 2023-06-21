#define TRACE
namespace Microsoft.Virtualization.Client.Management;

internal abstract class VMIntegrationComponentView : VMDeviceView, IVMIntegrationComponent, IVMDevice, IVirtualizationManagementObject
{
    internal static class WmiIntegrationComponentPropertyNames
    {
        public const string Enabled = "EnabledState";

        public const string Status = "OperationalStatus";

        public const string StatusDescriptions = "StatusDescriptions";
    }

    private const ushort gm_Enabled = 2;

    private const ushort gm_Disabled = 3;

    public bool Enabled
    {
        get
        {
            switch (GetProperty<ushort>("EnabledState"))
            {
            case 2:
                return true;
            default:
                VMTrace.TraceWarning("Enabled property of Integration Component Device is set to an unrecognized value. This is a bug on the server. Return it as disabled.");
                break;
            case 3:
                break;
            }
            return false;
        }
    }

    public VMIntegrationComponentOperationalStatus[] GetOperationalStatus()
    {
        return VMIntegrationComponentStatusUtilities.ConvertOperationalStatus(GetProperty<ushort[]>("OperationalStatus"));
    }

    public string[] GetOperationalStatusDescriptions()
    {
        return GetProperty<string[]>("StatusDescriptions");
    }
}

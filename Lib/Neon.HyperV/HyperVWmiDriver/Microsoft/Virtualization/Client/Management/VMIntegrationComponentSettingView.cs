#define TRACE
using System.Globalization;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class VMIntegrationComponentSettingView : VMDeviceSettingView, IVMIntegrationComponentSetting, IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable
{
    internal static class WmiIntegrationComponentSettingPropertyNames
    {
        public const string Enabled = "EnabledState";
    }

    private const ushort gm_Enabled = 2;

    private const ushort gm_Disabled = 3;

    public bool Enabled
    {
        get
        {
            ushort property = GetProperty<ushort>("EnabledState");
            switch (property)
            {
            case 2:
                return true;
            default:
                VMTrace.TraceWarning(string.Format(CultureInfo.InvariantCulture, "Enabled property of Integration Component RASD is set to an unrecognized value: '{0}'. This is a bug on the server. Return it as disabled.", property));
                break;
            case 3:
                break;
            }
            return false;
        }
        set
        {
            ushort num = (ushort)(value ? 2 : 3);
            SetProperty("EnabledState", num);
        }
    }

    public IVMIntegrationComponent GetIntegrationComponent()
    {
        return GetRelatedObject<IVMIntegrationComponent>(base.Associations.ElementSettingData, throwIfNotFound: false);
    }
}

namespace Microsoft.Virtualization.Client.Management;

internal class VMSerialPortSettingView : VMDeviceSettingView, IVMSerialPortSetting, IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable
{
    internal static class WmiMemberNames
    {
        public const string AttachedResourcePath = "Connection";

        public const string DebuggerMode = "DebuggerMode";
    }

    public string AttachedResourcePath
    {
        get
        {
            object[] property = GetProperty<object[]>("Connection");
            if (property != null && property.Length != 0)
            {
                return (string)property[0];
            }
            return null;
        }
        set
        {
            SetProperty("Connection", new string[1] { value ?? string.Empty });
        }
    }

    public bool DebuggerMode
    {
        get
        {
            return GetProperty<bool>("DebuggerMode");
        }
        set
        {
            SetProperty("DebuggerMode", value);
        }
    }
}

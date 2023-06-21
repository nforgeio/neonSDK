namespace Microsoft.Virtualization.Client.Management;

internal class VMSyntheticDisplayControllerSettingView : VMDeviceSettingView, IVMSyntheticDisplayControllerSetting, IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable, IDeleteableAsync, IDeleteable
{
    internal static class WmiMemberNames
    {
        public const string ResolutionType = "ResolutionType";

        public const string HorizontalResolution = "HorizontalResolution";

        public const string VerticalResolution = "VerticalResolution";
    }

    public ResolutionType ResolutionType
    {
        get
        {
            return (ResolutionType)GetProperty<byte>("ResolutionType");
        }
        set
        {
            SetProperty("ResolutionType", (byte)value);
        }
    }

    public int HorizontalResolution
    {
        get
        {
            return NumberConverter.UInt16ToInt32(GetProperty<ushort>("HorizontalResolution"));
        }
        set
        {
            ushort num = NumberConverter.Int32ToUInt16(value);
            SetProperty("HorizontalResolution", num);
        }
    }

    public int VerticalResolution
    {
        get
        {
            return NumberConverter.UInt16ToInt32(GetProperty<ushort>("VerticalResolution"));
        }
        set
        {
            ushort num = NumberConverter.Int32ToUInt16(value);
            SetProperty("VerticalResolution", num);
        }
    }
}

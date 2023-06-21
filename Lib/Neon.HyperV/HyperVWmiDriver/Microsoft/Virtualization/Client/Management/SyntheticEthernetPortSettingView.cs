namespace Microsoft.Virtualization.Client.Management;

internal class SyntheticEthernetPortSettingView : EthernetPortSettingView, ISyntheticEthernetPortSetting, IEthernetPortSetting, IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable, IDeleteableAsync, IDeleteable
{
    internal static class WmiMemberNames
    {
        public const string AllowPacketDirect = "AllowPacketDirect";

        public const string NumaAwarePlacement = "NumaAwarePlacement";

        public const string InterruptModeration = "InterruptModeration";
    }

    public override VMDeviceSettingType VMDeviceSettingType => VMDeviceSettingType.EthernetPortSynthetic;

    public IFailoverNetworkAdapterSetting FailoverNetworkAdapterSetting => GetRelatedObject<IFailoverNetworkAdapterSetting>(base.Associations.EthernetPortToFailoverNetwork, throwIfNotFound: false);

    public bool DeviceNamingEnabled
    {
        get
        {
            return GetPropertyOrDefault("DeviceNamingEnabled", defaultValue: false);
        }
        set
        {
            SetProperty("DeviceNamingEnabled", value);
        }
    }

    public uint MediaType => GetPropertyOrDefault("MediaType", 0u);

    public bool AllowPacketDirect
    {
        get
        {
            return GetPropertyOrDefault("AllowPacketDirect", defaultValue: false);
        }
        set
        {
            SetProperty("AllowPacketDirect", value);
        }
    }

    public bool NumaAwarePlacement
    {
        get
        {
            return GetPropertyOrDefault("NumaAwarePlacement", defaultValue: false);
        }
        set
        {
            SetProperty("NumaAwarePlacement", value);
        }
    }

    public bool InterruptModeration
    {
        get
        {
            return GetPropertyOrDefault("InterruptModeration", defaultValue: true);
        }
        set
        {
            SetProperty("InterruptModeration", value);
        }
    }
}

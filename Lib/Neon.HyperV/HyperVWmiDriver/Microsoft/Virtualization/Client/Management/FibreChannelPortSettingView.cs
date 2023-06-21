namespace Microsoft.Virtualization.Client.Management;

internal class FibreChannelPortSettingView : ResourcePoolAllocationSettingView, IFibreChannelPortSetting, IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable, IDeleteableAsync, IDeleteable
{
    internal static class FcPortAllocationSettingWmiPropertyNames
    {
        public const string VirtualPortWWNN = "VirtualPortWWNN";

        public const string VirtualPortWWPN = "VirtualPortWWPN";

        public const string SecondaryVirtualPortWWNN = "SecondaryWWNN";

        public const string SecondaryVirtualPortWWPN = "SecondaryWWPN";
    }

    public override VMDeviceSettingType VMDeviceSettingType => VMDeviceSettingType.FibreChannelPort;

    public string WorldWideNodeName
    {
        get
        {
            return GetProperty<string>("VirtualPortWWNN");
        }
        set
        {
            if (value == null)
            {
                value = string.Empty;
            }
            SetProperty("VirtualPortWWNN", value);
        }
    }

    public string WorldWidePortName
    {
        get
        {
            return GetProperty<string>("VirtualPortWWPN");
        }
        set
        {
            if (value == null)
            {
                value = string.Empty;
            }
            SetProperty("VirtualPortWWPN", value);
        }
    }

    public string SecondaryWorldWideNodeName
    {
        get
        {
            return GetProperty<string>("SecondaryWWNN");
        }
        set
        {
            if (value == null)
            {
                value = string.Empty;
            }
            SetProperty("SecondaryWWNN", value);
        }
    }

    public string SecondaryWorldWidePortName
    {
        get
        {
            return GetProperty<string>("SecondaryWWPN");
        }
        set
        {
            if (value == null)
            {
                value = string.Empty;
            }
            SetProperty("SecondaryWWPN", value);
        }
    }

    public IFcPoolAllocationSetting GetConnectionConfiguration()
    {
        return GetRelatedObject<IFcPoolAllocationSetting>(base.Associations.FibreChannelAdapterSettingToConnectionSetting, throwIfNotFound: false);
    }
}

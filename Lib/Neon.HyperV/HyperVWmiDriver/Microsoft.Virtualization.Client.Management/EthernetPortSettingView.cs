namespace Microsoft.Virtualization.Client.Management;

internal abstract class EthernetPortSettingView : VMDeviceSettingView, IEthernetPortSetting, IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable, IDeleteableAsync, IDeleteable
{
	internal static class WmiPropertyNames
	{
		public const string IsNetworkAddressStatic = "StaticMacAddress";

		public const string NetworkAddress = "Address";

		public const string ClusterMonitored = "ClusterMonitored";

		public const string DeviceNamingEnabled = "DeviceNamingEnabled";

		public const string MediaType = "MediaType";
	}

	public bool IsNetworkAddressStatic
	{
		get
		{
			return GetProperty<bool>("StaticMacAddress");
		}
		set
		{
			SetProperty("StaticMacAddress", value);
		}
	}

	public bool ClusterMonitored
	{
		get
		{
			return GetProperty<bool>("ClusterMonitored");
		}
		set
		{
			SetProperty("ClusterMonitored", value);
		}
	}

	public string NetworkAddress
	{
		get
		{
			return GetProperty<string>("Address");
		}
		set
		{
			if (value == null)
			{
				value = string.Empty;
			}
			SetProperty("Address", value);
		}
	}

	public IEthernetPort EthernetDevice => GetRelatedObject<IEthernetPort>(base.Associations.LogicalDeviceToSetting, throwIfNotFound: false);

	public IVMBootEntry BootEntry => GetRelatedObject<IVMBootEntry>(base.Associations.LogicalIdentity);

	public IEthernetConnectionAllocationRequest GetConnectionConfiguration()
	{
		return GetRelatedObject<IEthernetConnectionAllocationRequest>(base.Associations.EthernetPortSettingToConnectionSetting, throwIfNotFound: false);
	}

	public IGuestNetworkAdapterConfiguration GetGuestNetworkAdapterConfiguration()
	{
		return GetRelatedObject<IGuestNetworkAdapterConfiguration>(base.Associations.EthernetPortSettingToGuestNetworkAdapterConfiguration, throwIfNotFound: false);
	}
}

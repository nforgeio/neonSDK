namespace Microsoft.Virtualization.Client.Management;

internal class VMS3DisplayControllerSettingView : VMDeviceSettingView, IVMS3DisplayControllerSetting, IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable, IDeleteable
{
	internal static class WmiPropertyNames
	{
		public const string Address = "Address";
	}

	public string Address
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
}

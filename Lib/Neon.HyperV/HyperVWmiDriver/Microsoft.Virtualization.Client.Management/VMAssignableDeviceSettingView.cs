namespace Microsoft.Virtualization.Client.Management;

internal class VMAssignableDeviceSettingView : PciExpressPoolAllocationSettingView, IVMAssignableDeviceSetting, IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable, IDeleteableAsync, IDeleteable
{
	internal static class WmiSettingPropertyNames
	{
		public const string VirtualFunction = "VirtualFunctions";
	}

	public WmiObjectPath PhysicalDevicePath
	{
		get
		{
			WmiObjectPath result = null;
			if (base.VirtualDevice is IVMAssignableDevice iVMAssignableDevice)
			{
				result = iVMAssignableDevice.LogicalIdentity.ManagementPath;
			}
			else
			{
				string[] property = GetProperty<string[]>("HostResource");
				if (property != null && property.Length != 0)
				{
					string path = property[0];
					result = GetWmiObjectPathFromPath(path);
				}
			}
			return result;
		}
		set
		{
			SetProperty("HostResource", new string[1] { (value != null) ? value.ToString() : string.Empty });
		}
	}

	public ushort VirtualFunction
	{
		get
		{
			ushort[] property = GetProperty<ushort[]>("VirtualFunctions");
			ushort result = 0;
			if (property != null && property.Length != 0)
			{
				result = property[0];
			}
			return result;
		}
		set
		{
			SetProperty("VirtualFunctions", new ushort[1] { value });
		}
	}

	public override VMDeviceSettingType VMDeviceSettingType => VMDeviceSettingType.PciExpress;

	public IVMAssignableDevice GetPhysicalDevice()
	{
		IVMAssignableDevice result = null;
		WmiObjectPath physicalDevicePath = PhysicalDevicePath;
		if (physicalDevicePath != null)
		{
			result = (IVMAssignableDevice)GetViewFromPath(physicalDevicePath);
		}
		return result;
	}
}

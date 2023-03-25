namespace Microsoft.Virtualization.Client.Management;

internal abstract class VMDeviceView : View, IVMDevice, IVirtualizationManagementObject
{
	internal static class WmiDevicePropertyNames
	{
		public const string FriendlyName = "ElementName";

		public const string DeviceId = "DeviceID";
	}

	public string FriendlyName => GetProperty<string>("ElementName");

	public virtual string DeviceId => GetProperty<string>("DeviceID");

	public IVMComputerSystem VirtualComputerSystem => GetRelatedObject<IVMComputerSystem>(base.Associations.DevicesSystem);

	public virtual IVMDeviceSetting VirtualDeviceSetting => GetRelatedObject<IVMDeviceSetting>(base.Associations.LogicalDeviceToSetting);
}

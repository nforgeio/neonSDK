namespace Microsoft.Virtualization.Client.Management;

internal class VMAssignableDeviceView : VMDeviceView, IVMAssignableDevice, IVMDevice, IVirtualizationManagementObject
{
	internal static class WmiPropertyNames
	{
		public const string DeviceInstancePath = "DeviceInstancePath";

		public const string LocationPath = "LocationPath";
	}

	public string DeviceInstancePath => GetProperty<string>("DeviceInstancePath");

	public string LocationPath => GetProperty<string>("LocationPath");

	public IVMAssignableDevice LogicalIdentity => GetRelatedObject<IVMAssignableDevice>(base.Associations.LogicalIdentity);
}

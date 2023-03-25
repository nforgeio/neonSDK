namespace Microsoft.Virtualization.Client.Management;

internal class VMBiosView : VMDeviceView, IVMBios, IVMDevice, IVirtualizationManagementObject
{
	internal static class WmiPropertyNames
	{
		public const string SoftwareElementId = "SoftwareElementID";

		public const string ClassName = "Msvm_BIOSElement";
	}

	public override string DeviceId => GetProperty<string>("SoftwareElementID");

	public override IVMDeviceSetting VirtualDeviceSetting => null;
}

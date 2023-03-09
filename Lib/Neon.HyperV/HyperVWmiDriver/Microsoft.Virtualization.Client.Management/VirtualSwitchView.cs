namespace Microsoft.Virtualization.Client.Management;

internal abstract class VirtualSwitchView : View, IVirtualSwitch, IVirtualizationManagementObject, IPutable
{
	internal static class WmiMemberNames
	{
		public const string InstanceId = "Name";

		public const string FriendlyName = "ElementName";
	}

	public string InstanceId => GetProperty<string>("Name");

	public string FriendlyName => GetProperty<string>("ElementName");
}

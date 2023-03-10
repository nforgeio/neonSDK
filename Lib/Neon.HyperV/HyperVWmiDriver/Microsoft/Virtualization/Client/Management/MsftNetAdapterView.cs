namespace Microsoft.Virtualization.Client.Management;

internal sealed class MsftNetAdapterView : View, IMsftNetAdapter, IVirtualizationManagementObject
{
	internal static class WmiMemberNames
	{
		public const string InstanceId = "InstanceId";
	}

	public string DeviceId => GetProperty<string>("InstanceId");
}

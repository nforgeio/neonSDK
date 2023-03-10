namespace Microsoft.Virtualization.Client.Management;

internal class VMBootEntryView : View, IVMBootEntry, IVirtualizationManagementObject
{
	internal static class WmiPropertyNames
	{
		public const string Description = "BootSourceDescription";

		public const string SourceType = "BootSourceType";

		public const string DevicePath = "FirmwareDevicePath";

		public const string FilePath = "OtherLocation";
	}

	public string Description => GetProperty<string>("BootSourceDescription");

	public BootEntryType SourceType => (BootEntryType)GetProperty<uint>("BootSourceType");

	public string DevicePath => GetProperty<string>("FirmwareDevicePath");

	public string FilePath
	{
		get
		{
			if (SourceType != BootEntryType.File)
			{
				return null;
			}
			return GetProperty<string>("OtherLocation");
		}
	}

	public IVMDeviceSetting GetBootDeviceSetting()
	{
		if (SourceType == BootEntryType.File || SourceType == BootEntryType.Unknown)
		{
			return null;
		}
		return GetRelatedObject<IVMDeviceSetting>(base.Associations.LogicalIdentity);
	}
}

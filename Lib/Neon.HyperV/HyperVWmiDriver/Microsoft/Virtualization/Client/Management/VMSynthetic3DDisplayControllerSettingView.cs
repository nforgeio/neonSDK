namespace Microsoft.Virtualization.Client.Management;

internal class VMSynthetic3DDisplayControllerSettingView : VMDeviceSettingView, IVMSynthetic3DDisplayControllerSetting, IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable, IDeleteableAsync, IDeleteable
{
	internal static class WmiPropertyNames
	{
		public const string MaximumScreenResolution = "MaximumScreenResolution";

		public const string MaximumMonitors = "MaximumMonitors";

		public const string VRAMSizeBytes = "VRAMSizeBytes";
	}

	public int MaximumScreenResolution
	{
		get
		{
			return GetProperty<byte>("MaximumScreenResolution");
		}
		set
		{
			SetProperty("MaximumScreenResolution", NumberConverter.Int32ToByte(value));
		}
	}

	public int MaximumMonitors
	{
		get
		{
			return GetProperty<byte>("MaximumMonitors");
		}
		set
		{
			SetProperty("MaximumMonitors", NumberConverter.Int32ToByte(value));
		}
	}

	public ulong VRAMSizeBytes
	{
		get
		{
			return GetProperty<ulong>("VRAMSizeBytes");
		}
		set
		{
			SetProperty("VRAMSizeBytes", value);
		}
	}

	public override VMDeviceSettingType VMDeviceSettingType => VMDeviceSettingType.Synth3dVideo;
}

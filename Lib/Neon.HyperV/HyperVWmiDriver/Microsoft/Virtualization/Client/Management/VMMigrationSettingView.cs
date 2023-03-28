using System.Linq;

namespace Microsoft.Virtualization.Client.Management;

internal class VMMigrationSettingView : View, IVMMigrationSetting, IVirtualizationManagementObject
{
	internal static class WmiMemberNames
	{
		public const string MigrationType = "MigrationType";

		public const string DestinationPlannedVirtualSystemId = "DestinationPlannedVirtualSystemId";

		public const string DestinationIPAddressList = "DestinationIPAddressList";

		public const string RetainVhdCopiesOnSource = "RetainVhdCopiesOnSource";

		public const string EnableCompression = "EnableCompression";

		public const string TransportType = "TransportType";

		public const string UnmanagedVhds = "UnmanagedVhds";

		public const string RemoveSourceUnmanagedVhds = "RemoveSourceUnmanagedVhds";
	}

	public VMMigrationType MigrationType
	{
		get
		{
			return (VMMigrationType)NumberConverter.UInt16ToInt32(GetProperty<ushort>("MigrationType"));
		}
		set
		{
			ushort num = NumberConverter.Int32ToUInt16((int)value);
			SetProperty("MigrationType", num);
		}
	}

	public string DestinationPlannedVirtualSystemId
	{
		get
		{
			return GetProperty<string>("DestinationPlannedVirtualSystemId");
		}
		set
		{
			SetProperty("DestinationPlannedVirtualSystemId", value);
		}
	}

	public string[] DestinationIPAddressList
	{
		get
		{
			return GetProperty<string[]>("DestinationIPAddressList");
		}
		set
		{
			SetProperty("DestinationIPAddressList", value);
		}
	}

	public bool EnableCompression
	{
		get
		{
			return GetProperty<bool>("EnableCompression");
		}
		set
		{
			SetProperty("EnableCompression", value);
		}
	}

	public bool RetainVhdCopiesOnSource
	{
		get
		{
			return GetPropertyOrDefault("RetainVhdCopiesOnSource", defaultValue: false);
		}
		set
		{
			SetProperty("RetainVhdCopiesOnSource", value);
		}
	}

	public VMMigrationTransportType TransportType
	{
		get
		{
			return (VMMigrationTransportType)NumberConverter.UInt16ToInt32(GetProperty<ushort>("TransportType"));
		}
		set
		{
			ushort num = NumberConverter.Int32ToUInt16((int)value);
			SetProperty("TransportType", num);
		}
	}

	public MoveUnmanagedVhd[] UnmanagedVhds
	{
		get
		{
			return (from embeddedInstance in GetProperty<string[]>("UnmanagedVhds")
				select EmbeddedInstance.ConvertTo<MoveUnmanagedVhd>(base.Server, embeddedInstance)).ToArray();
		}
		set
		{
			string[] value2 = value.Select((MoveUnmanagedVhd instance) => instance.ToString()).ToArray();
			SetProperty("UnmanagedVhds", value2);
		}
	}

	public bool RemoveSourceUnmanagedVhds
	{
		get
		{
			return GetPropertyOrDefault("RemoveSourceUnmanagedVhds", defaultValue: false);
		}
		set
		{
			SetProperty("RemoveSourceUnmanagedVhds", value);
		}
	}
}

using System.Globalization;

namespace Microsoft.Virtualization.Client.Management;

internal class Win32DiskDriveView : View, IWin32DiskDrive, IVirtualizationManagementObject
{
	internal static class WmiMemberNames
	{
		public const string DiskNumber = "Index";

		public const string LunId = "SCSILogicalUnit";

		public const string PathId = "SCSIBus";

		public const string PortNumber = "SCSIPort";

		public const string TargetId = "SCSITargetId";

		public const string DeviceId = "DeviceID";
	}

	public string DeviceId => GetProperty<string>("DeviceID");

	public uint DiskNumber => GetProperty<uint>("Index");

	public ushort LunId => GetProperty<ushort>("SCSILogicalUnit");

	public uint PathId => GetProperty<uint>("SCSIBus");

	public ushort PortNumber => GetProperty<ushort>("SCSIPort");

	public ushort TargetId => GetProperty<ushort>("SCSITargetId");

	public IMountedStorageImage GetMountedStorageImage()
	{
		string format = "SELECT * FROM {0} WHERE {1}=\"{2}\" AND {3}=\"{4}\" AND {5}=\"{6}\" AND {7}=\"{8}\"";
		format = string.Format(CultureInfo.InvariantCulture, format, "Msvm_MountedStorageImage", "PathId", PathId, "Lun", LunId, "PortNumber", PortNumber, "TargetId", TargetId);
		QueryAssociation association = QueryAssociation.CreateFromQuery(base.Server.VirtualizationNamespace, format);
		return GetRelatedObject<IMountedStorageImage>(association, throwIfNotFound: false);
	}
}

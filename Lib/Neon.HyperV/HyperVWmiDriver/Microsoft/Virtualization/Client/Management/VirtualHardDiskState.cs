namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_VirtualHardDiskState")]
internal class VirtualHardDiskState : EmbeddedInstance
{
	internal static class WmiPropertyNames
	{
		public const string FileSize = "FileSize";

		public const string InUse = "InUse";

		public const string MinInternalSize = "MinInternalSize";

		public const string PhysicalSectorSize = "PhysicalSectorSize";

		public const string Alignment = "Alignment";

		public const string FragmentationPercentage = "FragmentationPercentage";
	}

	public long FileSize => NumberConverter.UInt64ToInt64(GetProperty("FileSize", 0uL));

	public bool InUse => GetProperty("InUse", defaultValue: false);

	public ulong? MinInternalSize => GetProperty<ulong?>("MinInternalSize");

	public long Alignment => NumberConverter.UInt32ToInt64(GetProperty("Alignment", 0u));

	public uint? FragmentationPercentage => GetProperty<uint?>("FragmentationPercentage");

	public VirtualHardDiskState()
	{
	}

	public VirtualHardDiskState(Server server, long fileSize, bool inUse, ulong? minInternalSize, long physicalSectorSize, long alignment, uint? fragmentationPercentage)
		: base(server, server.VirtualizationNamespace, "Msvm_VirtualHardDiskState")
	{
		AddProperty("FileSize", fileSize);
		AddProperty("InUse", inUse);
		AddProperty("MinInternalSize", minInternalSize);
		AddProperty("PhysicalSectorSize", physicalSectorSize);
		AddProperty("Alignment", alignment);
		AddProperty("FragmentationPercentage", fragmentationPercentage);
	}
}

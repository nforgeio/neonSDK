using Microsoft.Virtualization.Client.Management;

namespace Microsoft.Vhd.PowerShell;

internal sealed class VirtualHardDisk
{
	private readonly VirtualHardDiskSettingData m_Setting;

	private readonly VirtualHardDiskState m_State;

	private readonly string m_ComputerName;

	public string ComputerName => m_ComputerName;

	public string Path => m_Setting.Path;

	public VhdFormat VhdFormat => (VhdFormat)m_Setting.DiskFormat;

	public VhdType VhdType => (VhdType)m_Setting.DiskType;

	public ulong FileSize => NumberConverter.Int64ToUInt64(m_State.FileSize);

	public ulong Size => NumberConverter.Int64ToUInt64(m_Setting.MaxInternalSize);

	public ulong? MinimumSize => m_State.MinInternalSize;

	public uint LogicalSectorSize => NumberConverter.Int64ToUInt32(m_Setting.LogicalSectorSize);

	public uint PhysicalSectorSize => NumberConverter.Int64ToUInt32(m_Setting.PhysicalSectorSize);

	public uint BlockSize => NumberConverter.Int64ToUInt32(m_Setting.BlockSize);

	public string ParentPath => m_Setting.ParentPath;

	public string DiskIdentifier => m_Setting.VirtualDiskIdentifier;

	public uint? FragmentationPercentage => m_State.FragmentationPercentage;

	public uint Alignment => NumberConverter.Int64ToUInt32(m_State.Alignment);

	public bool Attached => m_State.InUse;

	public uint? DiskNumber { get; private set; }

	public bool IsPMEMCompatible => m_Setting.IsPmemCompatible;

	public VirtualHardDiskPmemAddressAbstractionType AddressAbstractionType => m_Setting.PmemAddressAbstractionType;

	internal VirtualHardDisk(Server server, VirtualHardDiskSettingData setting, VirtualHardDiskState state, uint? diskNumber)
	{
		m_ComputerName = server.UserSpecifiedName;
		m_Setting = setting;
		m_State = state;
		DiskNumber = diskNumber;
	}
}

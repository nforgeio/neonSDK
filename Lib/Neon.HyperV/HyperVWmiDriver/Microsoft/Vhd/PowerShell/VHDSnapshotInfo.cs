using System;
using System.Collections.Generic;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.Vhd.PowerShell;

internal sealed class VHDSnapshotInfo
{
	private readonly VHDSnapshotInformation m_Information;

	internal Server Server => m_Information.Server;

	public string ComputerName => Server.UserSpecifiedName;

	public string FilePath => m_Information.FilePath;

	public Guid SnapshotId => Guid.Parse(m_Information.SnapshotId);

	public string SnapshotPath => m_Information.SnapshotPath;

	public DateTime CreationTime => m_Information.CreationTime;

	public string ResilientChangeTrackingId => m_Information.RCTId;

	public IReadOnlyCollection<string> ParentPathsList => m_Information.ParentPathsList;

	internal VHDSnapshotInformation GetInformation()
	{
		return m_Information;
	}

	internal VHDSnapshotInfo(VHDSnapshotInformation information)
	{
		m_Information = information;
	}
}

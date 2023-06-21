using System.Collections.Generic;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.Vhd.PowerShell;

internal sealed class VHDSetInfo
{
    private readonly VHDSetInformation m_Information;

    public string ComputerName => m_Information.Server.UserSpecifiedName;

    public string Path => m_Information.Path;

    public IReadOnlyCollection<string> SnapshotIdList => m_Information.SnapshotIdList;

    public IReadOnlyCollection<string> AllPaths => m_Information.AllPaths;

    internal VHDSetInfo(VHDSetInformation information)
    {
        m_Information = information;
    }
}

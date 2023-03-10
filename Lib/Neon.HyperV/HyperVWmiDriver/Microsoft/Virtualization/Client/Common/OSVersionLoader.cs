using System;

namespace Microsoft.Virtualization.Client.Common;

internal static class OSVersionLoader
{
	private static readonly Version gm_Windows8Version = new Version(6, 2);

	private static readonly Version gm_WindowsBlueVersion = new Version(6, 3);

	private static readonly Version gm_LatestVersion = new Version(6, 4);

	internal static Version Windows8Version => gm_Windows8Version;

	internal static Version WindowsBlueVersion => gm_WindowsBlueVersion;

	internal static Version LatestVersion => gm_LatestVersion;

	internal static HyperVOSVersion GetHyperVOSVersion(Version windowsVersion)
	{
		HyperVOSVersion result = HyperVOSVersion.Unsupported;
		if (windowsVersion >= LatestVersion)
		{
			result = HyperVOSVersion.WindowsThreshold;
		}
		else if (windowsVersion >= WindowsBlueVersion)
		{
			result = HyperVOSVersion.WindowsBlue;
		}
		else if (windowsVersion >= Windows8Version)
		{
			result = HyperVOSVersion.Windows8;
		}
		return result;
	}
}

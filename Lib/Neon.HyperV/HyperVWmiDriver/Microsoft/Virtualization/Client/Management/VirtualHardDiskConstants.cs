using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Virtualization.Client.Management;

internal static class VirtualHardDiskConstants
{
	private const long gm_MinimumSizeInBytes = 3145728L;

	private const long gm_OneGBInBytes = 1073741824L;

	private const int gm_MinimumSizeInGB = 1;

	private const int gm_DefaultSizeInGB = 127;

	private const long gm_VhdMaximumSizeInBytes = 2190433320960L;

	private const long gm_VhdxMaximumSizeInBytes = 70368744177664L;

	private const int gm_VhdMaximumSizeInGB = 2040;

	private const int gm_VhdxMaximumSizeInGB = 65536;

	private const int gm_VhdxMaximumSizeInTB = 64;

	public static long MinimumSizeInBytes => 3145728L;

	public static long OneGBInBytes => 1073741824L;

	public static int MinimumSizeInGB => 1;

	public static int DefaultSizeInGB => 127;

	public static long VhdMaximumSizeInBytes => 2190433320960L;

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
	public static long VhdxMaximumSizeInBytes => 70368744177664L;

	public static int VhdMaximumSizeInGB => 2040;

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
	public static int VhdxMaximumSizeInGB => 65536;

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
	public static int VhdxMaximumSizeInTB => 64;
}

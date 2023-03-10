namespace Microsoft.Virtualization.Client.Management;

internal static class VMMemoryBoundValues
{
	public static readonly int SizeLowerBoundInMb = 32;

	public static readonly int SizeUpperBoundLegacyInMb = 1048576;

	public static readonly int SizeUpperBoundInMbRS5 = 12582912;

	public static readonly int SizeUpperBoundInMb19H1 = SizeUpperBoundInMbRS5;

	public static readonly int SizeUpperBoundInMb = 251658240;
}

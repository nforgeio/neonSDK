using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal static class RemoteFXUtilities
{
	public const string gm_DefaultAdapterId = "5353,00000000,00";

	public const string gm_Dx11AdapterId = "02C1,00000000,01";

	public const ulong MB = 1048576uL;

	private const uint BPP = 4u;

	private const uint MAX_PINNED_SURFACES = 5u;

	public static readonly List<Synth3DResolution> AllResolutions = new List<Synth3DResolution>
	{
		new Synth3DResolution(1024u, 768u),
		new Synth3DResolution(1280u, 1024u),
		new Synth3DResolution(1600u, 1200u),
		new Synth3DResolution(1920u, 1200u),
		new Synth3DResolution(2560u, 1600u),
		new Synth3DResolution(3840u, 2160u)
	};

	public static readonly List<byte> AllMonitorCounts = new List<byte> { 1, 2, 3, 4, 5, 6, 7, 8 };

	public static readonly List<Synth3DVramSize> AllVRAMSizes = new List<Synth3DVramSize>
	{
		new Synth3DVramSize(67108864uL),
		new Synth3DVramSize(134217728uL),
		new Synth3DVramSize(268435456uL),
		new Synth3DVramSize(536870912uL),
		new Synth3DVramSize(1073741824uL)
	};

	public static readonly List<Synth3DResolution> AllResolutions2012R2 = AllResolutions.GetRange(0, AllResolutions.Count - 1);

	private static readonly List<List<Synth3DResolution>> monitorToResolutions = new List<List<Synth3DResolution>>
	{
		AllResolutions.GetRange(0, AllResolutions.Count),
		AllResolutions.GetRange(0, AllResolutions.Count - 1),
		AllResolutions.GetRange(0, AllResolutions.Count - 2),
		AllResolutions.GetRange(0, AllResolutions.Count - 2),
		AllResolutions.GetRange(0, AllResolutions.Count - 4),
		AllResolutions.GetRange(0, AllResolutions.Count - 4),
		AllResolutions.GetRange(0, AllResolutions.Count - 4),
		AllResolutions.GetRange(0, AllResolutions.Count - 4)
	};

	public static List<Synth3DResolution> RetrieveResolutionList(byte monitorCount, int vmVersion)
	{
		int num = AllMonitorCounts.IndexOf(monitorCount);
		List<Synth3DResolution> result = null;
		if (num >= 0)
		{
			result = ((1 != monitorCount || vmVersion != VMConfigurationVersion.WinBlue.Major) ? monitorToResolutions[num] : AllResolutions.GetRange(0, AllResolutions.Count - 1));
		}
		return result;
	}

	public static List<Synth3DVramSize> RetrievePrunedVramList(ulong minRequiredVram)
	{
		int num = AllVRAMSizes.FindIndex((Synth3DVramSize o) => o.CompareTo(minRequiredVram) == 0);
		if (num == -1)
		{
			return null;
		}
		return AllVRAMSizes.GetRange(num, AllVRAMSizes.Count - num);
	}

	public static ulong RetrieveMinRequiredVramSize(int monitorIndex, int resolutionIndex, int vmVersion)
	{
		ulong result = 4294967295uL;
		if (vmVersion >= VMConfigurationVersion.WinThreshold_0.Major)
		{
			ulong requiredVramSize = (ulong)(AllResolutions[resolutionIndex].Horizontal * AllResolutions[resolutionIndex].Vertical * 4 * 5 * ((long)monitorIndex + 1L));
			result = AllVRAMSizes.Find((Synth3DVramSize obj) => obj.VramSize >= requiredVramSize)?.VramSize ?? AllVRAMSizes[AllVRAMSizes.Count - 1].VramSize;
		}
		else if (vmVersion == VMConfigurationVersion.WinBlue.Major)
		{
			ulong num = 268435456uL;
			ulong num2 = 134217728uL;
			int index = 3;
			int index2 = 2;
			int num3 = 0;
			uint horizontal = AllResolutions2012R2[resolutionIndex].Horizontal;
			uint vertical = AllResolutions2012R2[resolutionIndex].Vertical;
			result = ((horizontal > AllResolutions2012R2[index].Horizontal || vertical > AllResolutions2012R2[index2].Vertical || monitorIndex > num3) ? num : num2);
		}
		return result;
	}
}

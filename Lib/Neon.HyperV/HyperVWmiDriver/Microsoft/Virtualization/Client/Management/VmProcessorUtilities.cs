using System;
using Microsoft.Virtualization.Client.Common;

namespace Microsoft.Virtualization.Client.Management;

internal static class VmProcessorUtilities
{
	public static VpCountBoundValues GetVpCountBounds(Version vmVersion, VirtualSystemSubType subType, int hostSupportedProcessorCount)
	{
		VpCountBoundValues vpCountBoundValues = new VpCountBoundValues();
		vpCountBoundValues.LowerBound = VMVpCountBoundValues.LowerBoundCount;
		if (IsLegacyVM(vmVersion, subType))
		{
			vpCountBoundValues.UpperBound = VMVpCountBoundValues.UpperBoundCountLegacy;
		}
		else if (vmVersion <= VMConfigurationVersion.Redstone_5)
		{
			vpCountBoundValues.UpperBound = VMVpCountBoundValues.UpperBoundCountRS5;
		}
		else if (vmVersion <= VMConfigurationVersion.Release_19H1)
		{
			vpCountBoundValues.UpperBound = VMVpCountBoundValues.UpperBoundCount19H1;
			if (FeatureStaging.IsFeatureEnabled(Feature.AzureFeatureSet))
			{
				vpCountBoundValues.UpperBound = VMVpCountBoundValues.UpperBoundCount;
			}
		}
		else
		{
			vpCountBoundValues.UpperBound = VMVpCountBoundValues.UpperBoundCount;
		}
		vpCountBoundValues.UpperBound = Math.Min(vpCountBoundValues.UpperBound, hostSupportedProcessorCount);
		return vpCountBoundValues;
	}

	private static bool IsLegacyVM(Version vmVersion, VirtualSystemSubType subType)
	{
		if (vmVersion < VMConfigurationVersion.Redstone_1 || subType == VirtualSystemSubType.Type1)
		{
			return true;
		}
		return false;
	}

	public static int? GetDefaultHardwareThreadCount(Version vmVersion)
	{
		if (vmVersion < VMConfigurationVersion.WinThreshold_2)
		{
			return null;
		}
		if (vmVersion <= VMConfigurationVersion.Redstone_4)
		{
			return 1;
		}
		return 0;
	}
}

using System;
using Microsoft.Virtualization.Client.Common;

namespace Microsoft.Virtualization.Client.Management;

internal static class VmMemoryUtilities
{
	public static VmMemoryBounds GetMemoryBounds(Version vmVersion, VirtualSystemSubType subType)
	{
		VmMemoryBounds vmMemoryBounds = new VmMemoryBounds();
		vmMemoryBounds.LowerBoundInMb = VMMemoryBoundValues.SizeLowerBoundInMb;
		if (IsLegacyVM(vmVersion, subType))
		{
			vmMemoryBounds.UpperBoundInMb = VMMemoryBoundValues.SizeUpperBoundLegacyInMb;
		}
		else if (vmVersion <= VMConfigurationVersion.Redstone_5)
		{
			vmMemoryBounds.UpperBoundInMb = VMMemoryBoundValues.SizeUpperBoundInMbRS5;
		}
		else if (vmVersion <= VMConfigurationVersion.Release_19H1)
		{
			if (FeatureStaging.IsFeatureEnabled(Feature.AzureFeatureSet))
			{
				vmMemoryBounds.UpperBoundInMb = VMMemoryBoundValues.SizeUpperBoundInMb;
			}
			else
			{
				vmMemoryBounds.UpperBoundInMb = VMMemoryBoundValues.SizeUpperBoundInMb19H1;
			}
		}
		else
		{
			vmMemoryBounds.UpperBoundInMb = VMMemoryBoundValues.SizeUpperBoundInMb;
		}
		return vmMemoryBounds;
	}

	private static bool IsLegacyVM(Version vmVersion, VirtualSystemSubType subType)
	{
		if (vmVersion < VMConfigurationVersion.Redstone_1 || subType == VirtualSystemSubType.Type1)
		{
			return true;
		}
		return false;
	}
}

namespace Microsoft.Virtualization.Client.Common;

internal static class FeatureStaging
{
	internal static bool IsFeatureEnabled(Feature feature)
	{
		NativeMethods.IsFeatureEnabled(feature, out var IsEnabled);
		return IsEnabled;
	}
}

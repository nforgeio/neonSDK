using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal sealed class EthernetFeatureCapabilitiesView : View, IEthernetFeatureCapabilities, IVirtualizationManagementObject
{
	internal static class WmiMemberNames
	{
		public const string FeatureId = "FeatureId";
	}

	public string FeatureId => GetProperty<string>("FeatureId");

	public IEnumerable<IEthernetFeature> FeatureSettings => GetRelatedObjects<IEthernetFeature>(base.Associations.FeatureSettingsDefineCapabilities);
}

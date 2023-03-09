using Microsoft.Management.Infrastructure;

namespace Microsoft.Virtualization.Client.Management.Clustering;

internal class MSClusterResourceView : MSClusterResourceBaseView, IMSClusterResource, IMSClusterResourceBase, IVirtualizationManagementObject
{
	private CimInstance GetPrivateProperties()
	{
		return GetProperty<CimInstance>("PrivateProperties");
	}

	protected object GetInternalProperty(string propertyName)
	{
		return GetPrivateProperties().CimInstanceProperties[propertyName].Value;
	}

	protected void SetInternalProperty(string propertyName, object propertyValueToSet)
	{
		CimInstance privateProperties = GetPrivateProperties();
		privateProperties.CimInstanceProperties[propertyName].Value = propertyValueToSet;
		SetProperty("PrivateProperties", privateProperties);
	}

	public IMSClusterResourceGroup GetGroup()
	{
		return GetRelatedObject<IMSClusterResourceGroup>(base.Associations.ClusterGroupToResource);
	}
}

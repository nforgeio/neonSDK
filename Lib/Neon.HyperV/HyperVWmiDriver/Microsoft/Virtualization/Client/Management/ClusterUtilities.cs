#define TRACE
using System.Globalization;
using Microsoft.Virtualization.Client.Management.Clustering;

namespace Microsoft.Virtualization.Client.Management;

internal static class ClusterUtilities
{
	public static IMSClusterCluster GetUpdatedClusterCluster(Server server)
	{
		try
		{
			IMSClusterCluster clusterObject = ObjectLocator.GetClusterObject(server);
			clusterObject.UpdateAssociationCache();
			clusterObject.UpdatePropertyCache();
			return clusterObject;
		}
		catch (VirtualizationManagementException ex)
		{
			VMTrace.TraceError(string.Format(CultureInfo.InvariantCulture, "Exception in GetCluster for {0}.", server), ex);
			return null;
		}
	}
}

using System.Collections.Generic;
using System.Linq;
using Microsoft.HyperV.PowerShell.Common;
using Microsoft.Virtualization.Client.Management;

using ErrorMessages = Microsoft.HyperV.PowerShell.Common.ErrorMessages;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMSystemSwitchExtension : VirtualizationObject
{
	private readonly IDataUpdater<IInstalledEthernetSwitchExtension> m_Extension;

	public string Id => m_Extension.GetData(UpdatePolicy.EnsureUpdated).ExtensionId;

	public string Name => m_Extension.GetData(UpdatePolicy.EnsureUpdated).FriendlyName;

	public string Vendor => m_Extension.GetData(UpdatePolicy.EnsureUpdated).Company;

	public string Version => m_Extension.GetData(UpdatePolicy.EnsureUpdated).Version;

	public VMSwitchExtensionType ExtensionType => (VMSwitchExtensionType)m_Extension.GetData(UpdatePolicy.EnsureUpdated).ExtensionType;

	internal VMSystemSwitchExtension(IInstalledEthernetSwitchExtension extension)
		: base(extension)
	{
		m_Extension = InitializePrimaryDataUpdater(extension);
	}

	internal static IList<VMSystemSwitchExtension> GetExtensionsByNamesAndServers(IEnumerable<Server> servers, string[] requestedExtensionNames, bool allowWildcards, ErrorDisplayMode errorDisplayMode, IOperationWatcher operationWatcher)
	{
		_ = requestedExtensionNames;
		IList<VMSystemSwitchExtension> list = (from extension in servers.SelectMany((Server server) => ObjectLocator.QueryObjectsByNames<IInstalledEthernetSwitchExtension>(server, requestedExtensionNames, allowWildcards))
			select new VMSystemSwitchExtension(extension)).ToList();
		if (requestedExtensionNames != null && errorDisplayMode != 0)
		{
			VirtualizationObjectLocator.WriteNonMatchingNameErrors(requestedExtensionNames, list.Select((VMSystemSwitchExtension extension) => extension.Name), allowWildcards, ErrorMessages.VMSwitchExtension_NotFound, errorDisplayMode, operationWatcher);
		}
		return list;
	}
}

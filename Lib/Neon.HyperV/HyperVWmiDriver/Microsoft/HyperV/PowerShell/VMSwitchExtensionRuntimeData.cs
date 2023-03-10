using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal abstract class VMSwitchExtensionRuntimeData : VirtualizationObject
{
	protected IEthernetStatus m_RuntimeStatus;

	public string Name { get; private set; }

	public Guid? Id { get; private set; }

	public string ExtensionId { get; private set; }

	public string ExtensionName { get; private set; }

	public PSObject Data
	{
		get
		{
			IOrderedEnumerable<KeyValuePair<string, object>> orderedEnumerable = m_RuntimeStatus.Properties.OrderBy((KeyValuePair<string, object> kvp) => kvp.Key, StringComparer.OrdinalIgnoreCase);
			PSObject pSObject = new PSObject();
			foreach (KeyValuePair<string, object> item in orderedEnumerable)
			{
				pSObject.Properties.Add(new PSNoteProperty(item.Key, item.Value));
			}
			return pSObject;
		}
	}

	internal VMSwitchExtensionRuntimeData(IVirtualizationManagementObject runtimeStatus)
		: base(runtimeStatus)
	{
		IEthernetStatus runtimeStatus2 = runtimeStatus as IEthernetStatus;
		m_RuntimeStatus = runtimeStatus2;
		Name = m_RuntimeStatus.Name;
		ExtensionId = m_RuntimeStatus.ExtensionId;
		if (Guid.TryParse(m_RuntimeStatus.FeatureId, out var result))
		{
			Id = result;
		}
		IInstalledEthernetSwitchExtension installedEthernetSwitchExtension = ObjectLocator.GetInstalledEthernetSwitchExtension(base.Server, ExtensionId);
		if (installedEthernetSwitchExtension != null)
		{
			ExtensionName = installedEthernetSwitchExtension.FriendlyName;
		}
	}

	internal static IEnumerable<TStatus> FilterRuntimeStatuses<TStatus>(IEnumerable<TStatus> statuses, Guid[] featureIds, string[] featureNames, VMSwitchExtension[] extensions, string[] extensionNames) where TStatus : VMSwitchExtensionRuntimeData
	{
		if (featureIds != null)
		{
			statuses = statuses.Where((TStatus status) => status.Id.HasValue && featureIds.Contains(status.Id.Value));
		}
		else if (featureNames != null)
		{
			WildcardPatternMatcher featureMatcher = new WildcardPatternMatcher(featureNames);
			statuses = statuses.Where((TStatus status) => featureMatcher.MatchesAny(status.Name));
		}
		if (extensions != null)
		{
			statuses = statuses.Where((TStatus status) => extensions.Any((VMSwitchExtension extension) => string.Equals(extension.Id, status.ExtensionId, StringComparison.OrdinalIgnoreCase)));
		}
		else if (extensionNames != null)
		{
			WildcardPatternMatcher extensionMatcher = new WildcardPatternMatcher(extensionNames);
			statuses = statuses.Where((TStatus status) => extensionMatcher.MatchesAny(status.ExtensionName));
		}
		return statuses;
	}
}

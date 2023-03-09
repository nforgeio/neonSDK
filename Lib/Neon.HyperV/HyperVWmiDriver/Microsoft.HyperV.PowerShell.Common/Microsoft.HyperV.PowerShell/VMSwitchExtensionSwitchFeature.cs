using System.Collections.Generic;
using System.Linq;
using Microsoft.HyperV.PowerShell.Common;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMSwitchExtensionSwitchFeature : VMSwitchExtensionCustomFeature
{
	internal VMSwitch ParentSwitch { get; private set; }

	protected override string DescriptionForPut => TaskDescriptions.SetVMSwitchFeature;

	protected override string DescriptionForRemove => TaskDescriptions.RemoveVMSwitchFeature;

	internal VMSwitchExtensionSwitchFeature(IEthernetSwitchFeature featureSetting, VMSwitch parentSwitch)
		: base(featureSetting, GetFeatureServiceFromSwitch(parentSwitch))
	{
		ParentSwitch = parentSwitch;
	}

	protected override void ResetParentFeatureCache()
	{
		ParentSwitch.InvalidateFeatureCache();
	}

	internal static IEnumerable<VMSwitchExtensionSwitchFeature> GetTemplateSwitchFeatures(Server server)
	{
		return from feature in ObjectLocator.GetHostComputerSystem(server).GetAllFeatures(Capabilities.DefaultCapability).OfType<IEthernetSwitchFeature>()
			select new VMSwitchExtensionSwitchFeature(feature, null);
	}

	private static IVirtualSwitchManagementService GetFeatureServiceFromSwitch(VMSwitch parentSwitch)
	{
		if (!(parentSwitch != null))
		{
			return null;
		}
		return ObjectLocator.GetVirtualSwitchManagementService(parentSwitch.Server);
	}
}

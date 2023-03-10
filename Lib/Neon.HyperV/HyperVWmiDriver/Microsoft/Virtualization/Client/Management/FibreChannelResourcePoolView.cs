using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Virtualization.Client.Management;

internal class FibreChannelResourcePoolView : ResourcePoolView, IFibreChannelResourcePool, IResourcePool, IVirtualizationManagementObject, IDeleteableAsync, IDeleteable, IMetricMeasurableElement
{
	public IFcPoolAllocationSetting FcPoolConnectionAllocationSetting
	{
		get
		{
			IFcPoolAllocationSetting relatedObject = GetRelatedObject<IFcPoolAllocationSetting>(base.Associations.ResourcePoolToAllocationSetting);
			relatedObject.IsPoolRasd = true;
			return relatedObject;
		}
	}

	public IEnumerable<IVirtualFcSwitch> GetVirtualFcSwitches(bool updateRasdPropertyCache)
	{
		IFcPoolAllocationSetting fcPoolConnectionAllocationSetting = FcPoolConnectionAllocationSetting;
		if (updateRasdPropertyCache)
		{
			fcPoolConnectionAllocationSetting.UpdatePropertyCache();
		}
		return fcPoolConnectionAllocationSetting.GetSwitches().Cast<IVirtualFcSwitch>();
	}

	public bool HasSwitch(IVirtualFcSwitch virtualSwitch)
	{
		return (from s in GetVirtualFcSwitches(updateRasdPropertyCache: false)
			select s.ManagementPath).Contains(virtualSwitch.ManagementPath);
	}
}

using System.Collections.Generic;
using Microsoft.HyperV.PowerShell.Common;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class DataExchangeComponent : VMIntegrationComponent
{
	public bool IsClustered => ((IVMDataExchangeComponentSetting)m_IntegrationComponentSetting.GetData(UpdatePolicy.EnsureUpdated)).VMIsClustered;

	internal override string PutDescription => TaskDescriptions.SetVMDataExchangeComponent;

	internal DataExchangeComponent(IVMDataExchangeComponentSetting setting, VirtualMachineBase parentVirtualMachineObject)
		: base(setting, parentVirtualMachineObject)
	{
	}

	internal IEnumerable<DataExchangeItem> GetHostOnlyKeyValuePairItems()
	{
		return ((IVMDataExchangeComponentSetting)m_IntegrationComponentSetting.GetData(UpdatePolicy.EnsureUpdated)).GetHostOnlyKeyValuePairItems();
	}
}

using System.Collections.Generic;
using System.Linq;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal static class MetricUtilities
{
	internal enum MetricServiceState : ushort
	{
		Unknown = 0,
		Other = 1,
		Enabled = 2,
		Disabled = 3,
		Reset = 4,
		PartiallyEnabled = 32768
	}

	private static readonly VMNetworkAdapterAclDirection[] gm_AclDirections = new VMNetworkAdapterAclDirection[2]
	{
		VMNetworkAdapterAclDirection.Inbound,
		VMNetworkAdapterAclDirection.Outbound
	};

	private static readonly string[] gm_AclIPAddresses = new string[2] { "0.0.0.0/0", "::/0" };

	internal static void ChangeMetricServiceState(MetricServiceState requestedState, IMeasurable measurableObject)
	{
		IMetricMeasurableElement measurableElement = measurableObject.GetMeasurableElement(UpdatePolicy.None);
		ObjectLocator.GetMetricService(measurableElement.Server).ControlMetrics(measurableElement, null, (MetricEnabledState)requestedState);
		measurableElement.InvalidateAssociationCache();
	}

	internal static void EnablePortAclMetrics(IEnumerable<VMNetworkAdapterAclSetting> aclSettings)
	{
		IMetricService metricService = null;
		foreach (VMNetworkAdapterAclSetting aclSetting in aclSettings)
		{
			if (metricService == null)
			{
				metricService = ObjectLocator.GetMetricService(aclSetting.Server);
			}
			IEthernetSwitchPortAclFeature ethernetSwitchPortAclFeature = (IEthernetSwitchPortAclFeature)aclSetting.m_FeatureSetting;
			metricService.ControlMetrics(ethernetSwitchPortAclFeature, null, MetricEnabledState.Enabled);
			ethernetSwitchPortAclFeature.InvalidateAssociationCache();
		}
	}

	internal static void ConfigureMeteringPortAcls(VirtualMachine vm, IOperationWatcher operationWatcher)
	{
		foreach (VMNetworkAdapter adapter in vm.GetNetworkAdapters())
		{
			if (!adapter.AclList.Any((VMNetworkAdapterAclSetting acl) => acl.Action == VMNetworkAdapterAclAction.Meter))
			{
				IEnumerable<VMNetworkAdapterAclSetting> source = from direction in gm_AclDirections
					from address in gm_AclIPAddresses
					select CreateTemplateAcl(adapter, direction, address);
				adapter.AddFeatureSettings(source.ToArray(), operationWatcher);
			}
		}
	}

	private static VMNetworkAdapterAclSetting CreateTemplateAcl(VMNetworkAdapter parentAdapter, VMNetworkAdapterAclDirection direction, string address)
	{
		VMNetworkAdapterAclSetting vMNetworkAdapterAclSetting = VMNetworkAdapterAclSetting.CreateTemplateAclSetting(parentAdapter);
		vMNetworkAdapterAclSetting.Action = VMNetworkAdapterAclAction.Meter;
		vMNetworkAdapterAclSetting.Direction = direction;
		vMNetworkAdapterAclSetting.SetAddress(address, isRemote: true, isMacAddress: false);
		return vMNetworkAdapterAclSetting;
	}
}

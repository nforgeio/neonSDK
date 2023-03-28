using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;

namespace Microsoft.Virtualization.Client.Management;

internal static class NetworkingUtilities
{
	private const string gm_VmqQueryString = "SELECT * FROM Msft_NetAdapterVmqQueueSettingDataWHERE InterfaceDescription = '{0}' AND QueueId = {1}";

	private const string gm_VirtualFunctionQueryString = "SELECT * FROM Msft_NetAdapterSriovVfSettingData WHERE InterfaceDescription = '{0}' AND FunctionId = {1}";

	[SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "This should only be called to create connections for Ethernet switches andexternal network ports.")]
	public static IEthernetPortAllocationSettingData[] CreateTemplateSwitchConnections(Server server, IVirtualEthernetSwitch virtualSwitch, IExternalNetworkPort[] externalNetworkPorts, bool addExternalPort, bool addInternalPort, bool copyMacAddressToInternalPort)
	{
		IHostComputerSystem hostComputerSystem = ObjectLocator.GetHostComputerSystem(server);
		List<IEthernetPortAllocationSettingData> list = new List<IEthernetPortAllocationSettingData>(2);
		if (addExternalPort)
		{
			IEthernetPortAllocationSettingData item = CreateTemplateSwitchPortSetting(hostComputerSystem, virtualSwitch.FriendlyName + "_External", externalNetworkPorts.Select((IExternalNetworkPort e) => e.ManagementPath).ToArray(), null);
			list.Add(item);
		}
		if (addInternalPort)
		{
			string macAddress = null;
			if (copyMacAddressToInternalPort)
			{
				macAddress = externalNetworkPorts[0].PermanentAddress;
			}
			IEthernetPortAllocationSettingData item2 = CreateTemplateSwitchPortSetting(hostComputerSystem, virtualSwitch.FriendlyName, new WmiObjectPath[1] { hostComputerSystem.ManagementPath }, macAddress);
			list.Add(item2);
		}
		return list.ToArray();
	}

	public static IEthernetPortAllocationSettingData CreateTemplateSwitchPortSetting(IHostComputerSystem hostSystem, string portName, WmiObjectPath[] hostResourcePaths, string macAddress)
	{
		IEthernetPortAllocationSettingData obj = (IEthernetPortAllocationSettingData)hostSystem.GetSettingCapabilities(VMDeviceSettingType.EthernetConnection, Capabilities.DefaultCapability);
		obj.FriendlyName = portName;
		obj.HostResources = hostResourcePaths;
		obj.Address = macAddress;
		return obj;
	}

	public static void DisconnectSwitchInternal(Server server, IVirtualEthernetSwitchPort externalPort, List<IVirtualEthernetSwitchPort> internalPorts, bool disconnectInternalOnly)
	{
		bool num = externalPort != null;
		bool flag = internalPorts.Count > 0;
		List<IVirtualEthernetSwitchPort> list = new List<IVirtualEthernetSwitchPort>(internalPorts.Count + 1);
		if (num && !disconnectInternalOnly)
		{
			list.Add(externalPort);
		}
		if (flag)
		{
			list.AddRange(internalPorts);
		}
		IVirtualSwitchManagementService virtualSwitchManagementService = ObjectLocator.GetVirtualSwitchManagementService(server);
		using IVMTask iVMTask = virtualSwitchManagementService.BeginRemoveVirtualSwitchPorts(list.ToArray());
		iVMTask.WaitForCompletion();
		virtualSwitchManagementService.EndRemoveVirtualSwitchPorts(iVMTask);
	}

	public static IExternalNetworkPort[] FindExternalNetworkPorts(Server server, string[] externalNicNames, string[] externalNicDescriptions, TimeSpan updateThreshold)
	{
		IVirtualSwitchManagementService switchService = ObjectLocator.GetVirtualSwitchManagementService(server);
		switchService.UpdateExternalNetworkPorts(updateThreshold);
		IEnumerable<IExternalNetworkPort> enumerable = ((externalNicNames == null || externalNicNames.Length == 0) ? externalNicDescriptions.Select((string desc) => switchService.ExternalNetworkPorts.First((IExternalNetworkPort port) => string.Equals(port.Name, desc, StringComparison.OrdinalIgnoreCase))) : (from name in externalNicNames
			select FindExternalPortDeviceId(server, name) into deviceId
			select switchService.ExternalNetworkPorts.First((IExternalNetworkPort port) => port.DeviceId.EndsWith(deviceId, StringComparison.OrdinalIgnoreCase))));
		if (enumerable == null)
		{
			throw ThrowHelper.CreateRelatedObjectNotFoundException(server, typeof(IExternalEthernetPort));
		}
		return enumerable.ToArray();
	}

	public static string FindExternalPortDeviceId(Server server, string netAdapterInterfaceName)
	{
		return (ObjectFactory.Instance.QueryVirtualizationManagementObjects<IMsftNetAdapter>(server, Server.StandardCimV2Namespace, string.Format(CultureInfo.InvariantCulture, "SELECT * FROM {0} WHERE NAME='{1}'", "Msft_NetAdapter", netAdapterInterfaceName)).SingleOrDefault() ?? throw new VirtualizationManagementException(string.Format(CultureInfo.CurrentCulture, ErrorMessages.NetworkUtilities_NicNotFound, netAdapterInterfaceName))).DeviceId;
	}

	public static ICimInstance GetVMQueue(Server server, string netAdapterInterfaceDescription, uint vmqId)
	{
		string query = string.Format(CultureInfo.InvariantCulture, "SELECT * FROM Msft_NetAdapterVmqQueueSettingDataWHERE InterfaceDescription = '{0}' AND QueueId = {1}", ManagementPathHelper.EscapePropertyValue(netAdapterInterfaceDescription, ManagementPathHelper.QuoteType.Single), vmqId);
		return server.QueryInstances(Server.StandardCimV2Namespace, query).SingleOrDefault();
	}

	public static ICimInstance GetVirtualFunction(Server server, string netAdapterInterfaceDescription, ushort virtualFunctionId)
	{
		string query = string.Format(CultureInfo.InvariantCulture, "SELECT * FROM Msft_NetAdapterSriovVfSettingData WHERE InterfaceDescription = '{0}' AND FunctionId = {1}", ManagementPathHelper.EscapePropertyValue(netAdapterInterfaceDescription, ManagementPathHelper.QuoteType.Single), virtualFunctionId);
		return server.QueryInstances(Server.StandardCimV2Namespace, query).SingleOrDefault();
	}
}

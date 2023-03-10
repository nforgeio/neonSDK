#define TRACE
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Microsoft.Virtualization.Client.Management;

internal class EthernetConnectionAllocationRequestView : EthernetPortAllocationSettingDataView, IEthernetConnectionAllocationRequest, IEthernetPortAllocationSettingData, IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable, IDeleteableAsync, IDeleteable
{
	internal static class ConnectionRequestMemberNames
	{
		public const string Parent = "Parent";

		public const string IsEnabled = "EnabledState";

		public const string RequiredFeatures = "RequiredFeatures";

		public const string RequiredFeatureNames = "RequiredFeatureHints";

		public const string DiagnoseNetworkConnectivity = "DiagnoseNetworkConnection";
	}

	private const ushort CimEnabledStateEnabled = 2;

	private const ushort CimEnabledStateDisabled = 3;

	IEthernetPortSetting IEthernetConnectionAllocationRequest.Parent
	{
		get
		{
			IEthernetPortSetting ethernetPortSetting = null;
			string property = GetProperty<string>("Parent");
			if (!string.IsNullOrEmpty(property))
			{
				return (IEthernetPortSetting)GetViewFromPath(property);
			}
			throw ThrowHelper.CreateRelatedObjectNotFoundException(base.Server, typeof(IEthernetPortSetting));
		}
		set
		{
			string value2 = string.Empty;
			if (value != null)
			{
				value2 = value.ManagementPath.ToString();
			}
			SetProperty("Parent", value2);
		}
	}

	public bool IsEnabled
	{
		get
		{
			return GetProperty<ushort>("EnabledState") == 2;
		}
		set
		{
			ushort num = (ushort)(value ? 2 : 3);
			SetProperty("EnabledState", num);
		}
	}

	public IResourcePool ResourcePool
	{
		get
		{
			string query = string.Format(CultureInfo.InvariantCulture, "SELECT * FROM {0} WHERE ResourceType = {1} AND PoolId = \"{2}\"", "Msvm_ResourcePool", 33, base.PoolId);
			QueryAssociation association = QueryAssociation.CreateFromQuery(base.Server.VirtualizationNamespace, query);
			return GetRelatedObject<IResourcePool>(association, throwIfNotFound: false);
		}
	}

	public IReadOnlyList<string> RequiredFeatureIds
	{
		get
		{
			return (from path in WmiObjectPath.FromStringArray(GetProperty<string[]>("RequiredFeatures"))
				select path.KeyValues["FeatureId"].ToString()).ToList();
		}
		set
		{
			IHostComputerSystem hostComputerSystem = ObjectLocator.GetHostComputerSystem(base.Server);
			Dictionary<string, IEthernetFeatureCapabilities> capabilitiesById = hostComputerSystem.EthernetFeatureCapabilities.ToDictionary((IEthernetFeatureCapabilities capability) => capability.FeatureId, StringComparer.OrdinalIgnoreCase);
			WmiObjectPath[] wmiObjectPaths = value.Select((string id) => capabilitiesById[id].ManagementPath).ToArray();
			SetProperty("RequiredFeatures", WmiObjectPath.ToStringArray(wmiObjectPaths));
		}
	}

	public IReadOnlyList<string> RequiredFeatureNames => GetProperty<string[]>("RequiredFeatureHints");

	public int TestNetworkConnectivity(bool isSender, string senderIPAddress, string receiverIPAddress, string receiverMacAddress, int isolationID, int sequenceNumber, int payloadSize)
	{
		IProxy serviceProxy = GetServiceProxy();
		VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Testing connectivity of network adapter '{0}' ('{1}').", base.DeviceId, base.FriendlyName));
		NetworkConnectionDiagnosticSettingData networkConnectionDiagnosticSettingData = new NetworkConnectionDiagnosticSettingData(base.Server, isSender, senderIPAddress, receiverIPAddress, receiverMacAddress, isolationID, sequenceNumber, payloadSize);
		object[] array = new object[4] { this, networkConnectionDiagnosticSettingData, null, null };
		uint num = serviceProxy.InvokeMethod("DiagnoseNetworkConnection", array);
		if (num != View.ErrorCodeSuccess)
		{
			throw ThrowHelper.CreateVirtualizationOperationFailedException(string.Format(CultureInfo.CurrentCulture, ErrorMessages.TestNetworkConnectivityFailed, base.FriendlyName), VirtualizationOperation.TestNetworkConnectivity, num, GetErrorCodeMapper(), null);
		}
		VMTrace.TraceUserActionCompleted("Test network connectivity succeeded.");
		return NumberConverter.UInt32ToInt32(EmbeddedInstance.ConvertTo<NetworkConnectionDiagnosticInformation>(base.Server, array[2] as string).RoundTripTime);
	}
}

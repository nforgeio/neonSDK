#define TRACE
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Virtualization.Client.Management;

internal class VirtualSwitchManagementServiceView : EthernetSwitchFeatureServiceView, IVirtualSwitchManagementService, IVirtualizationManagementObject, IEthernetSwitchFeatureService
{
	internal static class WmiMemberNames
	{
		public const string CreateSwitch = "DefineSystem";

		public const string DeleteSwitch = "DestroySystem";

		public const string AddSwitchPorts = "AddResourceSettings";

		public const string RemoveSwitchPorts = "RemoveResourceSettings";

		public const string ModifySwitchPorts = "ModifyResourceSettings";
	}

	public IEnumerable<IVirtualEthernetSwitch> VirtualSwitches
	{
		get
		{
			List<IVirtualEthernetSwitch> list = new List<IVirtualEthernetSwitch>();
			foreach (IVirtualEthernetSwitch relatedObject in GetRelatedObjects<IVirtualEthernetSwitch>(base.Associations.QueryVirtualSwitch))
			{
				if (!string.Equals(relatedObject.InstanceId, "161DF6ED-7CE7-450F-8DDB-4603FF64EDFC", StringComparison.OrdinalIgnoreCase))
				{
					list.Add(relatedObject);
				}
			}
			return list;
		}
	}

	public IEnumerable<IExternalNetworkPort> ExternalNetworkPorts
	{
		get
		{
			List<IExternalNetworkPort> list = new List<IExternalNetworkPort>();
			list.AddRange(GetRelatedObjects<IExternalNetworkPort>(base.Associations.QueryExternalEthernetPorts));
			list.AddRange(GetRelatedObjects<IExternalNetworkPort>(base.Associations.QueryWiFiPorts));
			return list;
		}
	}

	public IEnumerable<IInternalEthernetPort> InternalEthernetPorts => GetRelatedObjects<IInternalEthernetPort>(base.Associations.QueryInternalEthernetPorts);

	public IVirtualEthernetSwitchManagementCapabilities Capabilities => GetRelatedObject<IVirtualEthernetSwitchManagementCapabilities>(base.Associations.ElementCapabilities);

	public IVMTask BeginCreateVirtualSwitch(string friendlyName, string instanceId, string notes, bool iovPreferred, BandwidthReservationMode? bandwidthReservationMode, bool? packetDirectEnabled, bool? embeddedTeamingEnabled)
	{
		string text = MakeSwitchSettingEmbeddedInstance(friendlyName, instanceId, notes, iovPreferred, bandwidthReservationMode, packetDirectEnabled, embeddedTeamingEnabled);
		object[] array = new object[5] { text, null, null, null, null };
		string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.CreateSwitchFailed, friendlyName ?? string.Empty);
		VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Creating Ethernet switch '{0}'.", friendlyName ?? string.Empty));
		uint result = InvokeMethod("DefineSystem", array);
		IVMTask iVMTask = BeginNetworkMethodTaskReturn(result, array[3], array[4]);
		iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
		return iVMTask;
	}

	public IVirtualEthernetSwitch EndCreateVirtualSwitch(IVMTask task)
	{
		IVirtualEthernetSwitch result = EndMethodReturn<IVirtualEthernetSwitch>(task, VirtualizationOperation.CreateSwitch);
		VMTrace.TraceUserActionCompleted("Creating ethernet switch completed successfully.");
		return result;
	}

	public IVMTask BeginDeleteVirtualSwitch(IVirtualEthernetSwitch virtualSwitch)
	{
		if (virtualSwitch == null)
		{
			throw new ArgumentNullException("virtualSwitch");
		}
		string instanceId = virtualSwitch.InstanceId;
		string friendlyName = virtualSwitch.FriendlyName;
		friendlyName = friendlyName ?? string.Empty;
		string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.DeleteSwitchFailed, friendlyName);
		object[] array = new object[2] { virtualSwitch, null };
		VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Deleting switch '{0}' ('{1}')", instanceId, friendlyName));
		uint result = InvokeMethod("DestroySystem", array);
		IVMTask iVMTask = BeginNetworkMethodTaskReturn(result, null, array[1]);
		iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
		return iVMTask;
	}

	public void EndDeleteVirtualSwitch(IVMTask task)
	{
		EndMethod(task, VirtualizationOperation.DeleteSwitch);
		VMTrace.TraceUserActionCompleted("Switch deleted successfully.");
	}

	public IVMTask BeginAddVirtualSwitchPorts(IVirtualEthernetSwitch virtualSwitch, IEthernetPortAllocationSettingData[] portsToAdd)
	{
		if (virtualSwitch == null)
		{
			throw new ArgumentNullException("virtualSwitch");
		}
		if (portsToAdd == null)
		{
			throw new ArgumentNullException("portsToAdd");
		}
		string instanceId = virtualSwitch.InstanceId;
		string friendlyName = virtualSwitch.FriendlyName;
		friendlyName = friendlyName ?? string.Empty;
		string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.AddSwitchPortsFailed, friendlyName);
		VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Adding ports to switch '{0}' ('{1}').", instanceId, friendlyName));
		string[] array = new string[portsToAdd.Length];
		for (int i = 0; i < portsToAdd.Length; i++)
		{
			array[i] = portsToAdd[i].GetEmbeddedInstance();
		}
		object[] array2 = new object[4] { virtualSwitch.Setting, array, null, null };
		uint result = InvokeMethod("AddResourceSettings", array2);
		IVMTask iVMTask = BeginNetworkMethodTaskReturn(result, array2[2], array2[3]);
		iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
		return iVMTask;
	}

	public IEnumerable<IEthernetPortAllocationSettingData> EndAddVirtualSwitchPorts(IVMTask task)
	{
		IEnumerable<IEthernetPortAllocationSettingData> result = EndMethodReturnEnumeration<IEthernetPortAllocationSettingData>(task, VirtualizationOperation.AddSwitchPorts);
		VMTrace.TraceUserActionCompleted("Added switch ports successfully.");
		return result;
	}

	public IVMTask BeginRemoveVirtualSwitchPorts(IVirtualEthernetSwitchPort[] portsToRemove)
	{
		if (portsToRemove == null)
		{
			throw new ArgumentNullException("portsToRemove");
		}
		string deleteSwitchPortsFailed = ErrorMessages.DeleteSwitchPortsFailed;
		VMTrace.TraceUserActionInitiated("Removing switch ports.");
		IVirtualEthernetSwitchPortSetting[] array = new IVirtualEthernetSwitchPortSetting[portsToRemove.Length];
		for (int i = 0; i < portsToRemove.Length; i++)
		{
			array[i] = portsToRemove[i].Setting;
		}
		object[] array2 = new object[2] { array, null };
		uint result = InvokeMethod("RemoveResourceSettings", array2);
		IVMTask iVMTask = BeginNetworkMethodTaskReturn(result, null, array2[1]);
		iVMTask.ClientSideFailedMessage = deleteSwitchPortsFailed;
		return iVMTask;
	}

	public void EndRemoveVirtualSwitchPorts(IVMTask task)
	{
		EndMethod(task, VirtualizationOperation.RemoveSwitchPorts);
		VMTrace.TraceUserActionCompleted("Removing switch ports completed successfully.");
	}

	public IVMTask BeginModifyVirtualSwitchPorts(IVirtualEthernetSwitchPortSetting[] portsToModify)
	{
		if (portsToModify == null)
		{
			throw new ArgumentNullException("portsToModify");
		}
		string modifySwitchPortsFailed = ErrorMessages.ModifySwitchPortsFailed;
		VMTrace.TraceUserActionInitiated("Modifying switch ports.");
		string[] array = new string[portsToModify.Length];
		for (int i = 0; i < portsToModify.Length; i++)
		{
			array[i] = portsToModify[i].GetEmbeddedInstance();
		}
		object[] array2 = new object[3] { array, null, null };
		uint result = InvokeMethod("ModifyResourceSettings", array2);
		IVMTask iVMTask = BeginNetworkMethodTaskReturn(result, null, array2[2]);
		iVMTask.ClientSideFailedMessage = modifySwitchPortsFailed;
		return iVMTask;
	}

	public void EndModifyVirtualSwitchPorts(IVMTask task)
	{
		EndMethod(task, VirtualizationOperation.ModifySwitchPorts);
		VMTrace.TraceUserActionCompleted("Modifying switch ports completed successfully.");
	}

	public void UpdateSwitches(TimeSpan threshold)
	{
		base.Proxy.UpdateOneCachedAssociation(base.Associations.QueryVirtualSwitch, threshold);
	}

	public void UpdateExternalNetworkPorts(TimeSpan threshold)
	{
		base.Proxy.UpdateOneCachedAssociation(base.Associations.QueryExternalEthernetPorts, threshold);
		base.Proxy.UpdateOneCachedAssociation(base.Associations.QueryWiFiPorts, threshold);
	}

	public void UpdateInternalEthernetPorts(TimeSpan threshold)
	{
		base.Proxy.UpdateOneCachedAssociation(base.Associations.QueryInternalEthernetPorts, threshold);
	}

	private string MakeSwitchSettingEmbeddedInstance(string friendlyName, string instanceId, string notes, bool iovPreferred, BandwidthReservationMode? bandwidthReservationMode, bool? packetDirectEnabled, bool? embeddedTeamingEnabled)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
		dictionary.Add("ElementName", friendlyName);
		dictionary.Add("Notes", new string[1] { notes });
		dictionary.Add("IOVPreferred", iovPreferred);
		if (!string.IsNullOrEmpty(instanceId))
		{
			dictionary.Add("InstanceID", instanceId);
		}
		if (bandwidthReservationMode.HasValue)
		{
			dictionary.Add("BandwidthReservationMode", bandwidthReservationMode.Value);
		}
		if (packetDirectEnabled.HasValue)
		{
			dictionary.Add("PacketDirectEnabled", packetDirectEnabled.Value);
		}
		if (embeddedTeamingEnabled.HasValue)
		{
			dictionary.Add("TeamingEnabled", embeddedTeamingEnabled.Value);
		}
		return base.Server.GetNewEmbeddedInstance(base.ManagementPath.NamespaceName, "Msvm_VirtualEthernetSwitchSettingData", dictionary);
	}
}

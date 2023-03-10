#define TRACE
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Virtualization.Client.Management;

internal class EthernetSwitchFeatureServiceView : View, IEthernetSwitchFeatureService, IVirtualizationManagementObject
{
	internal static class EthernetSwitchFeatureServiceMembers
	{
		public const string AddFeatureSettings = "AddFeatureSettings";

		public const string ModifyFeatureSettings = "ModifyFeatureSettings";

		public const string RemoveFeatureSettings = "RemoveFeatureSettings";
	}

	public IVMTask BeginAddPortFeatures(IEthernetPortAllocationSettingData connectionRequest, IEthernetSwitchPortFeature[] features)
	{
		if (connectionRequest == null)
		{
			throw new ArgumentNullException("connectionRequest");
		}
		if (features == null)
		{
			throw new ArgumentNullException("features");
		}
		string[] featureEmbeddedInstances = features.Select((IEthernetSwitchPortFeature feature) => feature.GetEmbeddedInstance()).ToArray();
		return BeginAddPortFeatures(connectionRequest, featureEmbeddedInstances);
	}

	public IVMTask BeginAddPortFeatures(IEthernetPortAllocationSettingData connectionRequest, string[] featureEmbeddedInstances)
	{
		if (connectionRequest == null)
		{
			throw new ArgumentNullException("connectionRequest");
		}
		if (featureEmbeddedInstances == null)
		{
			throw new ArgumentNullException("featureEmbeddedInstances");
		}
		VMTrace.TraceUserActionInitiated("Adding features to an Ethernet port connection request.");
		return BeginAddFeaturesInternal(connectionRequest, featureEmbeddedInstances);
	}

	public IEnumerable<IEthernetSwitchPortFeature> EndAddPortFeatures(IVMTask task)
	{
		IEnumerable<IEthernetSwitchPortFeature> result = EndMethodReturnEnumeration<IEthernetSwitchPortFeature>(task, VirtualizationOperation.ConfigureFeatureSettings);
		VMTrace.TraceUserActionCompleted("Added feature settings to switch port connection successfully.");
		return result;
	}

	public IVMTask BeginAddSwitchFeatures(IVirtualEthernetSwitchSetting switchSetting, IEthernetSwitchFeature[] features)
	{
		if (switchSetting == null)
		{
			throw new ArgumentNullException("switchSetting");
		}
		if (features == null)
		{
			throw new ArgumentNullException("features");
		}
		string[] featureEmbeddedInstances = features.Select((IEthernetSwitchFeature feature) => feature.GetEmbeddedInstance()).ToArray();
		return BeginAddSwitchFeatures(switchSetting, featureEmbeddedInstances);
	}

	public IVMTask BeginAddSwitchFeatures(IVirtualEthernetSwitchSetting switchSetting, string[] featureEmbeddedInstances)
	{
		if (switchSetting == null)
		{
			throw new ArgumentNullException("switchSetting");
		}
		if (featureEmbeddedInstances == null)
		{
			throw new ArgumentNullException("featureEmbeddedInstances");
		}
		VMTrace.TraceUserActionInitiated("Adding features to an Ethernet switch.");
		return BeginAddFeaturesInternal(switchSetting, featureEmbeddedInstances);
	}

	public IEnumerable<IEthernetSwitchFeature> EndAddSwitchFeatures(IVMTask task)
	{
		IEnumerable<IEthernetSwitchFeature> result = EndMethodReturnEnumeration<IEthernetSwitchFeature>(task, VirtualizationOperation.ConfigureFeatureSettings);
		VMTrace.TraceUserActionCompleted("Added feature settings to switch successfully.");
		return result;
	}

	private IVMTask BeginAddFeaturesInternal(IVirtualizationManagementObject switchOrPortSetting, string[] featureEmbeddedInstances)
	{
		object[] array = new object[4] { switchOrPortSetting, featureEmbeddedInstances, null, null };
		uint result = InvokeMethod("AddFeatureSettings", array);
		IVMTask iVMTask = BeginMethodTaskReturn(result, array[2], array[3]);
		iVMTask.ClientSideFailedMessage = ErrorMessages.AddEthernetFeatureSettingsFailed;
		return iVMTask;
	}

	public IVMTask BeginModifyFeatures(IEthernetFeature[] features)
	{
		if (features == null)
		{
			throw new ArgumentNullException("features");
		}
		string[] featureEmbeddedInstances = features.Select((IEthernetFeature feature) => feature.GetEmbeddedInstance()).ToArray();
		return BeginModifyFeatures(featureEmbeddedInstances);
	}

	public IVMTask BeginModifyFeatures(string[] featureEmbeddedInstances)
	{
		VMTrace.TraceUserActionInitiated("Modifying features of an Ethernet port connection request or Ethernet switch.");
		object[] array = new object[3] { featureEmbeddedInstances, null, null };
		uint result = InvokeMethod("ModifyFeatureSettings", array);
		IVMTask iVMTask = BeginMethodTaskReturn(result, array[1], array[2]);
		iVMTask.ClientSideFailedMessage = ErrorMessages.ModifyEthernetFeatureSettingsFailed;
		return iVMTask;
	}

	public void EndModifyFeatures(IVMTask task)
	{
		EndMethod(task, VirtualizationOperation.ConfigureFeatureSettings);
		VMTrace.TraceUserActionCompleted("Modified feature settings to switch port connection successfully.");
	}

	public IVMTask BeginRemoveFeatures(IEthernetFeature[] features)
	{
		if (features == null)
		{
			throw new ArgumentNullException("features");
		}
		WmiObjectPath[] array = features.Select((IEthernetFeature feature) => feature.ManagementPath).ToArray();
		VMTrace.TraceUserActionInitiated("Removing features from an Ethernet port connection request or Ethernet switch.");
		object[] array2 = new object[2] { array, null };
		uint result = InvokeMethod("RemoveFeatureSettings", array2);
		IVMTask iVMTask = BeginMethodTaskReturn(result, null, array2[1]);
		iVMTask.ClientSideFailedMessage = ErrorMessages.RemoveEthernetFeatureSettingsFailed;
		return iVMTask;
	}

	public void EndRemoveFeatures(IVMTask task)
	{
		EndMethod(task, VirtualizationOperation.ConfigureFeatureSettings);
		VMTrace.TraceUserActionCompleted("Removed feature settings to switch port connection successfully.");
	}
}

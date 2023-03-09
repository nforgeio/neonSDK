#define TRACE
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Microsoft.Virtualization.Client.Management;

internal class ResourcePoolView : View, IResourcePool, IVirtualizationManagementObject, IDeleteableAsync, IDeleteable, IMetricMeasurableElement
{
	internal static class WmiResourcePoolPropertyNames
	{
		public const string PoolId = "PoolID";

		public const string ResourceType = "ResourceType";

		public const string ResourceSubtype = "ResourceSubType";

		public const string OtherResourceType = "OtherResourceType";

		public const string Primordial = "Primordial";

		public const string Delete = "DeletePool";
	}

	public string PoolId => GetProperty<string>("PoolID") ?? string.Empty;

	public VMDeviceSettingType DeviceSettingType
	{
		get
		{
			ushort property = GetProperty<ushort>("ResourceType");
			string propertyOrDefault = GetPropertyOrDefault<string>("ResourceSubType");
			string propertyOrDefault2 = GetPropertyOrDefault<string>("OtherResourceType");
			return VMDeviceSettingTypeMapper.MapResourceSubTypeToVMDeviceSettingType(property, propertyOrDefault, propertyOrDefault2);
		}
	}

	public bool Primordial => GetProperty<bool>("Primordial");

	public MetricEnabledState AggregateMetricEnabledState => MetricServiceView.CalculateAggregatedMetricEnabledState(GetRelatedObjects<IMeasuredElementToMetricDefinitionAssociation>(base.Associations.MeasuredElementToMetricDefRelationship));

	public IResourcePoolSetting Setting => GetRelatedObject<IResourcePoolSetting>(base.Associations.ResourcePoolToPoolSetting);

	public IEnumerable<IVMDeviceSetting> AllCapabilities => GetAllocationCapabilities().Capabilities;

	public IEnumerable<IVMDevice> PhysicalDevices => GetRelatedObjects<IVMDevice>(base.Associations.ResourcePoolPysicalDevices);

	public IEnumerable<IResourcePool> ParentPools => GetRelatedObjects<IResourcePool>(base.Associations.ParentPools);

	public IEnumerable<IResourcePool> ChildPools => GetRelatedObjects<IResourcePool>(base.Associations.ChildPools);

	public IEnumerable<IResourcePoolAllocationSetting> AllocationSettings
	{
		get
		{
			foreach (IResourcePoolAllocationSetting relatedObject in GetRelatedObjects<IResourcePoolAllocationSetting>(base.Associations.ResourcePoolToAllocationSetting))
			{
				relatedObject.IsPoolRasd = true;
				yield return relatedObject;
			}
		}
	}

	public override string ToString()
	{
		return PoolId;
	}

	void IVirtualizationManagementObject.UpdateAssociationCache()
	{
		UpdateAssociationCache();
		try
		{
			GetAllocationCapabilities().UpdateAssociationCache();
		}
		catch (ObjectNotFoundException ex)
		{
			VMTrace.TraceError("Could not find this resource pool's allocation capabilities!", ex);
		}
	}

	public IVMDeviceSetting GetCapabilities(SettingsDefineCapabilities capability)
	{
		if (capability == null)
		{
			throw new ArgumentNullException("capability");
		}
		IVMDeviceSetting iVMDeviceSetting = null;
		foreach (IVMDeviceSetting allCapability in AllCapabilities)
		{
			if (capability.MatchByDescription(allCapability.DeviceId))
			{
				iVMDeviceSetting = allCapability;
				break;
			}
		}
		if (iVMDeviceSetting == null)
		{
			throw ThrowHelper.CreateRelatedObjectNotFoundException(base.Server, typeof(IVMDeviceSetting));
		}
		return iVMDeviceSetting;
	}

	public IVMTask BeginDelete()
	{
		IProxy resourcePoolConfigurationServiceProxy = GetResourcePoolConfigurationServiceProxy();
		object[] array = new object[2] { this, null };
		VMTrace.TraceUserActionInitiated("Started Deleting Resource Pool...", string.Format(CultureInfo.InvariantCulture, "Resource Pool with PoolID '{0}'", PoolId));
		uint result = resourcePoolConfigurationServiceProxy.InvokeMethod("DeletePool", array);
		IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[1]);
		if (iVMTask != null)
		{
			string text2 = (iVMTask.ClientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.DeleteResourcePoolFailed, PoolId));
		}
		return iVMTask;
	}

	public void EndDelete(IVMTask deleteTask)
	{
		EndMethod(deleteTask, VirtualizationOperation.Delete);
		VMTrace.TraceUserActionCompleted("Delete ResourcePool completed successfully.");
	}

	public void Delete()
	{
		using IVMTask iVMTask = BeginDelete();
		iVMTask.WaitForCompletion();
		EndDelete(iVMTask);
	}

	private IAllocationCapabilities GetAllocationCapabilities()
	{
		return GetRelatedObject<IAllocationCapabilities>(base.Associations.ElementCapabilities);
	}

	public IReadOnlyCollection<IMetricValue> GetMetricValues()
	{
		return GetRelatedObjects<IMetricValue>(base.Associations.MeasuredElementToMetricValue).ToList();
	}
}

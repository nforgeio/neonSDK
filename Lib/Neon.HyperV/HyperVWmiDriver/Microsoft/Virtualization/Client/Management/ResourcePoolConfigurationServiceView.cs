#define TRACE
using System;
using System.Globalization;
using System.Linq;

namespace Microsoft.Virtualization.Client.Management;

internal class ResourcePoolConfigurationServiceView : View, IResourcePoolConfigurationService, IVirtualizationManagementObject
{
	internal static class WmiMemberNames
	{
		public const string CreatePool = "CreatePool";

		public const string ModifyPool = "ModifyPoolResources";
	}

	public IResourcePoolSetting CreateTemplatePoolSetting(string poolId, VMDeviceSettingType deviceType)
	{
		IResourcePoolSetting setting = ObjectLocator.GetHostComputerSystem(base.Server).GetPrimordialResourcePool(deviceType).Setting;
		setting.PoolId = poolId;
		return setting;
	}

	public IResourcePoolAllocationSetting CreateTemplateAllocationSetting(string poolId, VMDeviceSettingType deviceType)
	{
		return (IResourcePoolAllocationSetting)ObjectLocator.GetHostComputerSystem(base.Server).GetSettingCapabilities(deviceType, poolId, Capabilities.DefaultCapability);
	}

	public IVMTask BeginCreateResourcePool(IResourcePoolSetting resourcePoolSettingData, IResourcePool[] parentPools, IResourcePoolAllocationSetting[] resourceSettings)
	{
		if (resourcePoolSettingData.PoolId == null)
		{
			resourcePoolSettingData.PoolId = string.Empty;
		}
		object[] array = new object[resourceSettings.Length];
		for (int i = 0; i < resourceSettings.Length; i++)
		{
			array[i] = resourceSettings[i].GetEmbeddedInstance();
		}
		object[] array2 = new object[5]
		{
			resourcePoolSettingData.GetEmbeddedInstance(),
			parentPools,
			array,
			null,
			null
		};
		string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.CreatePool, resourcePoolSettingData.PoolId);
		VMTrace.TraceUserActionInitiated("Started Creating Pool...", string.Format(CultureInfo.InvariantCulture, "Resource Pool with PoolID '{0}'", resourcePoolSettingData.PoolId));
		uint result = InvokeMethod("CreatePool", array2);
		IVMTask iVMTask = BeginMethodTaskReturn(result, array2[3], array2[4]);
		iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
		return iVMTask;
	}

	public IResourcePool EndCreateResourcePool(IVMTask task)
	{
		IResourcePool result = EndMethodReturn<IResourcePool>(task, VirtualizationOperation.CreatePool);
		VMTrace.TraceUserActionCompleted("CreatePool completed successfully.");
		return result;
	}

	public IVMTask BeginModifyResourcePool(IResourcePool childPool, IResourcePool[] parentPools, IResourcePoolAllocationSetting[] resourceSettings)
	{
		if (childPool == null)
		{
			throw new ArgumentNullException("childPool");
		}
		if (parentPools == null)
		{
			throw new ArgumentNullException("parentPools");
		}
		if (resourceSettings == null)
		{
			throw new ArgumentNullException("resourceSettings");
		}
		object[] array = resourceSettings.Select((IResourcePoolAllocationSetting r) => r.GetEmbeddedInstance()).ToArray();
		object[] array2 = array;
		object[] array3 = new object[4] { childPool, parentPools, array2, null };
		string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.ModifyPoolFailed, childPool.PoolId);
		VMTrace.TraceUserActionInitiated("Starting to Modify Pool...", string.Format(CultureInfo.InvariantCulture, "Resource Pool with PoolID '{0}'", childPool.PoolId));
		uint result = InvokeMethod("ModifyPoolResources", array3);
		IVMTask iVMTask = BeginMethodTaskReturn(result, null, array3[3]);
		iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
		return iVMTask;
	}

	public void EndModifyResourcePool(IVMTask task)
	{
		EndMethod(task, VirtualizationOperation.ModifyPool);
		VMTrace.TraceUserActionCompleted("ModifyPool completed successfully.");
	}
}

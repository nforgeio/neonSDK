#define TRACE
using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal class VMStorageSettingView : View, IVMStorageSetting, IVirtualizationManagementObject, IPutableAsync, IPutable
{
	internal static class WmiMemberNames
	{
		public const string ThreadCountPerChannel = "ThreadCountPerChannel";

		public const string DisableInterruptBatching = "DisableInterruptBatching";

		public const string VirtualProcessorsPerChannel = "VirtualProcessorsPerChannel";

		public const string Put = "ModifySystemComponentSettings";
	}

	public ushort ThreadCountPerChannel
	{
		get
		{
			return GetProperty<ushort>("ThreadCountPerChannel");
		}
		set
		{
			SetProperty("ThreadCountPerChannel", value);
		}
	}

	public ushort VirtualProcessorsPerChannel
	{
		get
		{
			return GetProperty<ushort>("VirtualProcessorsPerChannel");
		}
		set
		{
			SetProperty("VirtualProcessorsPerChannel", value);
		}
	}

	public bool DisableInterruptBatching
	{
		get
		{
			return GetProperty<bool>("DisableInterruptBatching");
		}
		set
		{
			SetProperty("DisableInterruptBatching", value);
		}
	}

	protected override IVMTask BeginPutInternal(IDictionary<string, object> properties)
	{
		string embeddedInstance = GetEmbeddedInstance(properties);
		IProxy serviceProxy = GetServiceProxy();
		object[] array = new object[3]
		{
			new string[1] { embeddedInstance },
			null,
			null
		};
		VMTrace.TraceUserActionInitiatedWithProperties("Modifying storage settings.", properties);
		uint result = serviceProxy.InvokeMethod("ModifySystemComponentSettings", array);
		IVMTask iVMTask = BeginMethodTaskReturn(result, array[1], array[2]);
		iVMTask.PutProperties = properties;
		iVMTask.ClientSideFailedMessage = ErrorMessages.ModifyStorageSettingFailed;
		return iVMTask;
	}
}

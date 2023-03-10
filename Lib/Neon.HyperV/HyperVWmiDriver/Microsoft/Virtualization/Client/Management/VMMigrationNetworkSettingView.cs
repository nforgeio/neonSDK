#define TRACE
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Virtualization.Client.Management;

internal class VMMigrationNetworkSettingView : View, IVMMigrationNetworkSetting, IVirtualizationManagementObject, IPutableAsync, IPutable
{
	internal static class WmiMemberNames
	{
		public const string SubnetNumber = "SubnetNumber";

		public const string PrefixLength = "PrefixLength";

		public const string Metric = "Metric";

		public const string Tags = "Tags";
	}

	public string SubnetNumber
	{
		get
		{
			return GetProperty<string>("SubnetNumber");
		}
		set
		{
			SetProperty("SubnetNumber", value);
		}
	}

	public byte PrefixLength
	{
		get
		{
			return GetProperty<byte>("PrefixLength");
		}
		set
		{
			SetProperty("PrefixLength", value);
		}
	}

	public uint Metric
	{
		get
		{
			return GetProperty<uint>("Metric");
		}
		set
		{
			SetProperty("Metric", value);
		}
	}

	public string[] Tags
	{
		get
		{
			return GetProperty<string[]>("Tags");
		}
		set
		{
			SetProperty("Tags", value);
		}
	}

	protected override IVMTask BeginPutInternal(IDictionary<string, object> properties)
	{
		string embeddedInstance = GetEmbeddedInstance(properties);
		IProxy proxy = base.ProxyFactory.GetProxy(ObjectKeyCreator.CreateMigrationServiceObjectKey(base.Server), delayInitializePropertyCache: true);
		object[] array = new object[2]
		{
			new string[1] { embeddedInstance },
			null
		};
		VMTrace.TraceUserActionInitiatedWithProperties("Modifying migration network settings.", properties);
		uint result = proxy.InvokeMethod("ModifyNetworkSettings", array);
		IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[1]);
		iVMTask.PutProperties = properties;
		iVMTask.ClientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.ModifyMigrationNetworkSettingsFailed, base.Server);
		return iVMTask;
	}
}

#define TRACE
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Microsoft.Virtualization.Client.Management;

internal sealed class VirtualEthernetSwitchSettingView : View, IVirtualEthernetSwitchSetting, IVirtualizationManagementObject, IPutableAsync, IPutable
{
	internal static class WmiMemberNames
	{
		public const string SwitchFriendlyName = "ElementName";

		public const string SwitchId = "VirtualSystemIdentifier";

		public const string Notes = "Notes";

		public const string IOVPreferred = "IOVPreferred";

		public const string ExtensionOrder = "ExtensionOrder";

		public const string BandwidthReservationMode = "BandwidthReservationMode";

		public const string PacketDirectEnabled = "PacketDirectEnabled";

		public const string TeamingEnabled = "TeamingEnabled";

		public const string Put = "ModifySystemSettings";
	}

	public string InstanceId
	{
		get
		{
			return GetProperty<string>("InstanceID");
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				value = Guid.NewGuid().ToString().ToUpperInvariant();
			}
			SetProperty("InstanceID", value);
		}
	}

	public Guid Id => GetProperty("VirtualSystemIdentifier", WmiTypeConverters.GuidStringConverter);

	public string SwitchFriendlyName
	{
		get
		{
			return GetProperty<string>("ElementName");
		}
		set
		{
			if (value == null)
			{
				value = string.Empty;
			}
			SetProperty("ElementName", value);
		}
	}

	public string Notes
	{
		get
		{
			string result = string.Empty;
			string[] property = GetProperty<string[]>("Notes");
			if (property != null && property.Length != 0)
			{
				result = property[0];
			}
			return result;
		}
		set
		{
			if (value == null)
			{
				value = string.Empty;
			}
			string[] value2 = new string[1] { value };
			SetProperty("Notes", value2);
		}
	}

	public bool IOVPreferred
	{
		get
		{
			return GetProperty<bool>("IOVPreferred");
		}
		set
		{
			SetProperty("IOVPreferred", value);
		}
	}

	public IList<IEthernetSwitchExtension> ExtensionList
	{
		get
		{
			string[] property = GetProperty<string[]>("ExtensionOrder");
			if (property == null)
			{
				return new List<IEthernetSwitchExtension>();
			}
			return property.Select((string extensionPath) => GetViewFromPath(extensionPath)).Cast<IEthernetSwitchExtension>().ToList();
		}
		set
		{
			string[] value2 = ((value != null) ? value.Select((IEthernetSwitchExtension extension) => extension.ManagementPath.ToString()).ToArray() : new string[0]);
			SetProperty("ExtensionOrder", value2);
		}
	}

	public IVirtualEthernetSwitch Switch => GetRelatedObject<IVirtualEthernetSwitch>(base.Associations.SettingsDefineState);

	public BandwidthReservationMode BandwidthReservationMode => (BandwidthReservationMode)GetProperty<uint>("BandwidthReservationMode");

	public bool PacketDirectEnabled => GetProperty<bool>("PacketDirectEnabled");

	public bool TeamingEnabled => GetProperty<bool>("TeamingEnabled");

	public IEnumerable<IEthernetSwitchFeature> Features => GetRelatedObjects<IEthernetSwitchFeature>(base.Associations.EthernetSwitchSettingToFsd);

	protected override IVMTask BeginPutInternal(IDictionary<string, object> properties)
	{
		string instanceId = InstanceId;
		string text = (string)base.Proxy.GetProperty("ElementName");
		text = text ?? string.Empty;
		string embeddedInstance = GetEmbeddedInstance(properties);
		IProxy switchManagementServiceProxy = GetSwitchManagementServiceProxy();
		object[] array = new object[2] { embeddedInstance, null };
		string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.ModifyVirtualSwitchFailed, text);
		VMTrace.TraceUserActionInitiatedWithProperties(string.Format(CultureInfo.InvariantCulture, "Modifying switch '{0}' ('{1}')", instanceId, text), properties);
		uint result = switchManagementServiceProxy.InvokeMethod("ModifySystemSettings", array);
		IVMTask iVMTask = BeginNetworkMethodTaskReturn(result, null, array[1]);
		iVMTask.PutProperties = properties;
		iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
		return iVMTask;
	}
}

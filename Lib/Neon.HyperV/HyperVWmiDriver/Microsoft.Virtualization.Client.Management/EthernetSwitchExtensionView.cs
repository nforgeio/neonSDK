#define TRACE
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Virtualization.Client.Management;

internal class EthernetSwitchExtensionView : EthernetSwitchExtensionBaseView, IEthernetSwitchExtension, IPutableAsync, IPutable, IEthernetSwitchExtensionBase, IVirtualizationManagementObject
{
	internal static class WmiMemberNames
	{
		public const string Enabled = "EnabledState";

		public const string OperationalStatus = "OperationalStatus";

		public const string Put = "RequestStateChange";
	}

	private IVirtualEthernetSwitch m_Switch;

	private bool m_SwitchInitialized;

	public bool IsChild => Switch == null;

	public bool IsEnabled
	{
		get
		{
			return GetProperty<ushort>("EnabledState") == 2;
		}
		set
		{
			ushort num = (ushort)(value ? 2 : 3);
			if (!IsChild)
			{
				SetProperty("EnabledState", num);
			}
		}
	}

	public bool IsRunning
	{
		get
		{
			ushort[] property = GetProperty<ushort[]>("OperationalStatus");
			if (property != null && property.Length != 0)
			{
				return property[0] == 2;
			}
			return false;
		}
	}

	public IEnumerable<IEthernetSwitchExtension> Children
	{
		get
		{
			if (!IsChild)
			{
				return GetRelatedObjects<IEthernetSwitchExtension>(base.Associations.ParentToChildSwitchExtension);
			}
			return null;
		}
	}

	public IVirtualEthernetSwitch Switch
	{
		get
		{
			if (m_Switch == null && !m_SwitchInitialized)
			{
				m_Switch = GetRelatedObject<IVirtualEthernetSwitch>(base.Associations.EthernetSwitchExtensionToSwitch, throwIfNotFound: false);
				m_SwitchInitialized = true;
			}
			return m_Switch;
		}
	}

	public IEthernetSwitchExtension Parent
	{
		get
		{
			if (!IsChild)
			{
				return null;
			}
			return GetRelatedObject<IEthernetSwitchExtension>(base.Associations.ParentToChildSwitchExtension);
		}
	}

	protected override IVMTask BeginPutInternal(IDictionary<string, object> properties)
	{
		ushort property = GetProperty<ushort>("EnabledState");
		object[] array = new object[3] { property, null, null };
		string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.RequestSwitchExtensionStateChangeFailed, base.ExtensionId);
		VMTrace.TraceUserActionInitiatedWithProperties(string.Format(CultureInfo.InvariantCulture, "Changing enabled state of extension '{0}' to state '{1}'", base.ExtensionId, property), properties);
		uint result = InvokeMethod("RequestStateChange", array);
		IVMTask iVMTask = BeginNetworkMethodTaskReturn(result, null, array[1]);
		iVMTask.PutProperties = properties;
		iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
		return iVMTask;
	}
}

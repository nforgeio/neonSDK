#define TRACE
using System;
using System.Globalization;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class EthernetSwitchExtensionBaseView : View, IEthernetSwitchExtensionBase, IVirtualizationManagementObject
{
	internal static class BaseWmiMemberNames
	{
		public const string ExtensionId = "Name";

		public const string FriendlyName = "ElementName";

		public const string ExtensionType = "ExtensionType";

		public const string Company = "Vendor";

		public const string Version = "Version";

		public const string Description = "Description";
	}

	public string ExtensionId => GetProperty<string>("Name");

	public string FriendlyName => GetProperty<string>("ElementName");

	public EthernetSwitchExtensionType ExtensionType
	{
		get
		{
			EthernetSwitchExtensionType ethernetSwitchExtensionType = (EthernetSwitchExtensionType)GetProperty<byte>("ExtensionType");
			if (!Enum.IsDefined(typeof(EthernetSwitchExtensionType), ethernetSwitchExtensionType))
			{
				VMTrace.TraceError(string.Format(CultureInfo.CurrentCulture, ErrorMessages.InvalidPropertyValue, "ExtensionType"));
				ethernetSwitchExtensionType = EthernetSwitchExtensionType.Unknown;
			}
			return ethernetSwitchExtensionType;
		}
	}

	public string Company => GetProperty<string>("Vendor");

	public string Version => GetProperty<string>("Version");

	public string Description => GetProperty<string>("Description");
}

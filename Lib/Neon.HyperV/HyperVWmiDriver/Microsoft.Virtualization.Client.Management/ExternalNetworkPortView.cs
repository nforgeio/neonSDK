namespace Microsoft.Virtualization.Client.Management;

internal class ExternalNetworkPortView : EthernetPortView, IExternalNetworkPort, IEthernetPort, IVirtualSwitchPort, IVirtualizationManagementObject
{
	internal static class ExternalNetworkPortViewMembers
	{
		public const string IsBound = "IsBound";

		public const string Enabled = "EnabledState";
	}

	public bool IsBound => GetProperty<bool>("IsBound");

	public bool Enabled => GetProperty<ushort>("EnabledState") == 2;

	public IExternalEthernetPortCapabilities GetCapabilities()
	{
		return GetRelatedObject<IExternalEthernetPortCapabilities>(base.Associations.ElementCapabilities, throwIfNotFound: false);
	}
}

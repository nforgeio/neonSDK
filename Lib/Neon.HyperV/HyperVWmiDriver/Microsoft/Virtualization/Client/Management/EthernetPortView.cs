using System;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class EthernetPortView : VirtualSwitchPortView, IEthernetPort, IVirtualSwitchPort, IVirtualizationManagementObject
{
	internal static class WmiPropertyNames
	{
		public const string PermanentAddress = "PermanentAddress";

		public const string DeviceId = "DeviceId";
	}

	public string DeviceId => GetProperty<string>("DeviceId");

	public string PermanentAddress => GetProperty<string>("PermanentAddress");

	public ILanEndpoint LanEndpoint => GetRelatedObject<ILanEndpoint>(base.Associations.DeviceSAPImplementation, throwIfNotFound: false);

	public IEthernetPort GetConnectedEthernetPort(TimeSpan threshold)
	{
		IEthernetPort result = null;
		ILanEndpoint lanEndpoint = LanEndpoint;
		if (lanEndpoint != null)
		{
			lanEndpoint.UpdateAssociationCache(threshold);
			ILanEndpoint otherEndpoint = lanEndpoint.OtherEndpoint;
			if (otherEndpoint != null)
			{
				otherEndpoint.UpdateAssociationCache(threshold);
				result = otherEndpoint.EthernetPort;
			}
		}
		return result;
	}
}
